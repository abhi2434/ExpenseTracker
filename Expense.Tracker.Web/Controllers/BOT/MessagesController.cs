using Microsoft.Bot.Connector;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Expense.Tracker.Web.Models.Bot;
using System.Runtime.Serialization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Configuration;
using Expense.Tracker.Data;

namespace Expense.Tracker.Web.Controllers.BOT
{
    /// <summary>
    /// Solves the messages asked from BOTS. 
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        DataManager _databaseFactory;

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        /// <value>
        /// The database factory.
        /// </value>
        public DataManager DatabaseFactory
        {
            get
            {
                if (this._databaseFactory == null)
                {
                    string connection = ConfigurationManager.AppSettings["DBConnectionString"];
                    this._databaseFactory = new DataManager(connection);
                }
                return this._databaseFactory;
            }
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    string result = string.Empty;
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    Activity replyMessage = activity.CreateReply("APPSeCONNECT is typing...");
                    replyMessage.Type = ActivityTypes.Typing;
                    await this.SendTypingResponse(connector, replyMessage);

                    try
                    {
                        var handler = new BotCommandHandler();

                        var utilities = new BotUtil(activity, handler);

                        if (utilities.State > 2)
                            result = await handler.GetReply(activity, utilities.UserName, utilities.ClientState, utilities.State);

                        if (string.IsNullOrEmpty(result))
                            result = await utilities.GetReply(this.DatabaseFactory);
                    }
                    catch (SerializationException ex)
                    {
                        this.TrackTelemetry(ex);
                        if (ex.InnerException != null)
                            result = $"{ex.GetType().Name} : {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        this.TrackTelemetry(ex);
                        if (ex.InnerException != null)
                            result = $"{ex.GetType().Name} : {ex.Message}";

                        //result = "Thanks. :)";
                        //result = "Sorry, we have faced an issue. Can you please reframe the sentence and ask me again.";
                    }

                    if (!string.IsNullOrEmpty(result))
                    {
                        replyMessage.Type = ActivityTypes.Message;
                        replyMessage.Text = result;
                        await connector.Conversations.ReplyToActivityAsync(replyMessage);
                    }
                }
                else
                {
                    await this.HandleSystemMessage(activity);
                }
            }
            catch { }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;


        }

        private void TrackTelemetry(Exception ex)
        {
            var client = new TelemetryClient();

            var eTel = new ExceptionTelemetry(ex);
            eTel.Properties["controller"] = "Messages";
            eTel.Properties["action"] = "POST";
            eTel.Properties["username"] = "BOT";
            client.TrackException(eTel);
        }

        private async System.Threading.Tasks.Task SendTypingResponse(ConnectorClient connector, Activity activity)
        {
            await connector.Conversations.ReplyToActivityAsync(activity);
        }

        private async System.Threading.Tasks.Task HandleSystemMessage(Activity message)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            if (message.Type == ActivityTypes.DeleteUserData)
            {
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                var reply = message.CreateReply("Thank you for adding me to your contact list. I am APPSeCONNECT and I will help you to learn more about us. Ping me when you are free.");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                string response = "";
                switch (message.Action)
                {
                    case "add":
                        response = $"Thanks for adding {message.From.Name} to the conversation";
                        break;
                    case "remove":
                        response = $"Hmm, it seems you have removed {message.From.Name} from the conversation and he will not receive any messages from now.";
                        break;
                }
                await connector.Conversations.ReplyToActivityAsync(message.CreateReply(response));
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Nothing to do while user is typing... 
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                var reply = message.CreateReply("Pong");
                reply.Type = ActivityTypes.Ping;
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}
