using Expense.Tracker.Data;
using ExpenseTracker.Utilities;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense.Tracker.Web.Models.Bot
{
    public class BotUtil : IDisposable
    {
        public Activity CurrentActivity { get; set; }

        public StateClient ClientState
        {
            get
            {
                return this.CurrentActivity.GetStateClient();
            }
        }

        public string ChannelId
        {
            get
            {
                return this.CurrentActivity.ChannelId;
            }
        }
        public string UserId
        {
            get
            {
                return this.CurrentActivity.From.Id;
            }
        }

        public string ConversationId
        {
            get { return this.CurrentActivity.Conversation.Id; }
        }

        BotData _data;
        public BotData Data
        {
            get
            {

                this._data = this._data ?? this.ClientState.BotState.GetConversationData(this.ChannelId, this.ConversationId);
                return this._data;
            }
        }

        private string id = null;
        private string password = null;
        private MicrosoftAppCredentials botCred = null;
        private StateClient client = null;

        BotData _userdata;
        public BotData UserData
        {
            get
            {
                id = id ?? ConfigurationManager.AppSettings["MicrosoftAppId"];
                password = password ?? ConfigurationManager.AppSettings["MicrosoftAppPassword"];
                botCred = botCred ?? new MicrosoftAppCredentials(id, password);
                client = client ?? new StateClient(botCred);
                this._userdata = this._userdata ?? client.BotState.GetUserData(this.ChannelId, this.UserId);
                return this._userdata;
            }
        }


        public BotUtil(Activity theActivity, BotCommandHandler handler)
        {
            this.CurrentActivity = theActivity;
            this.CommandHandler = handler;
        }

        public async Task<string> GetReply(DataManager dataFactory)
        {
           
            int currentState = this.State;
            StringBuilder strReplyMessage = new StringBuilder();
            try
            {
                var reply = this.CommandHandler.GetResponseOnUnresolvedEntity(this.CurrentActivity, this.Data);
                if (!string.IsNullOrEmpty(reply))
                    return reply;

                //reply = this.CommandHandler.GetEmailNotifyAsked(this.CurrentActivity, this.UserName, this.UserEmail, this.Data, store);
                //if (!string.IsNullOrEmpty(reply))
                //    return reply;

                string userName = "not known";
                switch (currentState)
                {
                    case 0: // Fresh user
                        strReplyMessage.Append($"Hello, I am **APPSeCONNECT** Bot");
                        strReplyMessage.Append($"\n");
                        strReplyMessage.Append($"You can say anything");
                        strReplyMessage.Append($"\n");
                        strReplyMessage.Append($"to me and I will check whether I can respond");
                        strReplyMessage.Append($"\n\n");
                        strReplyMessage.Append($"Lets start with whether you are an existing user of APPSeCONNECT or not?");
                        this.State = 1;
                        break;
                    case 1:
                        bool isPositive = this.CommandHandler.DetectPositiveAnswer(this.CurrentActivity.Text);
                        if (isPositive)
                        {
                            BotAuthenticator botAuthenticator = new BotAuthenticator();
                            await botAuthenticator.ShowLoginDialog(this.CurrentActivity);
                            this.IsAuthenticationRequired = true;
                            strReplyMessage.Append($"After authentication ping me back for your queries ");
                        }
                        else
                        {
                            this.IsAuthenticationRequired = false;
                            strReplyMessage.Append("OK, so what should I call you? ");
                        }
                        this.State = 2;
                        break;
                    case 2: // User name check
                        var requireAuth = this.IsAuthenticationRequired;
                        if (requireAuth)
                        {
                            if (!string.IsNullOrEmpty(this.AccessToken))
                            {
                                strReplyMessage.Append($"Thanks {this.UserName}! Hope you are doing good. Tell me what do you want to know from me.");
                                this.State = 3;
                            }
                            else
                            {
                                strReplyMessage.Append("Sorry! You are currently in a restricted zone. Are you really an existing user of **APPSeCONNECT**?");
                                this.State = 1;
                            }
                        }
                        else
                        {
                            bool isUserName = Regex.IsMatch(this.CurrentActivity.Text, "^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]+$");
                            if (isUserName)
                            {
                                strReplyMessage.Append($"Hello {this.CurrentActivity.Text}!, May I ask your email id. I promise will not send you any mail without your concern. Please use valid email id.");
                                this.State = 3;
                                this.UserName = this.CurrentActivity.Text;
                                this.ClearProperty<int>("UserNameTries");
                            }
                            else
                            {
                                strReplyMessage.Append(await this.FailedUserNameNotify());
                                this.UserNameTries++;
                            }
                        }
                        break;
                    case 3:  // Redirect to Luis
                        if (this.IsAuthenticationRequired)
                        {
                            await Microsoft.Bot.Builder.Dialogs.Conversation.SendAsync(this.CurrentActivity, () => new BotModel(this.AccessToken, this.UserName, this.Data, this.UserEmail, this.CommandHandler, dataFactory));
                        }
                        else
                        {
                            RegexUtilities util = new RegexUtilities();
                            var emailText = Regex.Replace(this.CurrentActivity.Text, @"<\/?a[^>]*>", "");
                            if (util.IsValidEmail(emailText))
                            {
                                this.UserEmail = emailText;
                                this.State = 4;
                                strReplyMessage.Append("Great, so lets start asking me your queries. I am happy to help");
                            }
                            else
                            {
                                
                                await this.FailedEmailNotify(strReplyMessage, this.UserEmailTries, this.CurrentActivity);
                                this.UserEmailTries++;
                            }
                       }
                        break;
                    case 4:
                        if (!this.IsAuthenticationRequired)
                        {
                            await Microsoft.Bot.Builder.Dialogs.Conversation.SendAsync(this.CurrentActivity, () => new UnAuthorizedBotModel(this.AccessToken, this.UserName, this.UserEmail, this.Data, this.CommandHandler, dataFactory));
                        }
                        else
                        {
                            strReplyMessage.Append("Opps, it is unknown to me, please reframe the query once more.");
                        }
                        break;
                    default:
                        strReplyMessage.Append("Opps, it is unknown to me, please reframe the query once more.");
                        break;
                }

                userName = this.UserName;
                //store.ActivityLogger.AddChatMessage(this.UserId, this.ChannelId, userName, this.CurrentActivity.Text, strReplyMessage);
            }
            catch {  }
            finally
            {
                await this.ClientState.BotState.SetConversationDataAsync(
                           this.ChannelId, this.ConversationId, this.Data);
                
            }

            return strReplyMessage.ToString();
        }
        private async System.Threading.Tasks.Task FailedEmailNotify(StringBuilder strReplyMessage, int tries, Activity activity)
        {
            string text = activity.Text;
            switch (tries)
            {
                case 1:
                    strReplyMessage.Append($"Oh, I think you have mistyped the email id {text}, can you input it again");
                    break;
                case 2:
                    strReplyMessage.Append($"Nope, the  email id {text} is not valid, I am sorry about that, please input once more");
                    break;
                case 10:
                    strReplyMessage.Append("Sorry, you have tried a lot of emails. Lets not take this further. I am moving out, will talk later");
                    await this.ClearBotData();
                    break;
                default:
                    strReplyMessage.Append($"Are you trying out junk email ids ? Please dont, we might block you, please enter a proper email id. ");
                    break;
            }
        }

        /// <summary>
        /// This function is used to identify if the user given the name in correct format or not
        /// </summary>
        /// <returns>Username authentication message</returns>
        public async Task<string> FailedUserNameNotify()
        {
            string returnMessage = string.Empty;
            switch (this.UserNameTries)
            {
                case 1:
                    returnMessage = $"Oh, I don't think {this.UserName} is a valid name, please ensure you mention only name, can you try once more please.";
                    break;
                case 2:
                    returnMessage = $"Nope, the {this.UserName} cant be your name, Are you sure the whole phrase {this.UserName} is your name. Please retype";
                    break;
                case 10:
                    returnMessage = "Sorry, it seems you have forgot your name totally. Lets not take this further. I am moving out, will talk later";
                    await this.ClearBotData();
                    break;
                default:
                    returnMessage = $"Are you trying out junk user names like {this.UserName} ? Please dont, we might block you, please enter a proper name. ";
                    break;
            }
            return returnMessage;
        }

        #region UserData
        public int State
        {
            get
            {
                return this.Data.GetProperty<int>("State");
            }
            set
            {
                this.Data.SetProperty<int>("State", value);
            }
        }
        public string UserName
        {
            get
            {
                return this.Data.GetProperty<string>("UserName");
            }
            set
            {
                this.Data.SetProperty<string>("UserName", value);
            }
        }

        public string UserEmail
        {
            get
            {
                //return this.Data.GetProperty<string>("UserName");
                return this.Data.GetProperty<string>("UserEmail");
            }
            set
            {
                this.Data.SetProperty<string>("UserEmail", value);
            }
        }
        private int UserEmailTries
        {
            get
            {
                return this.Data.GetProperty<int>("UserEmailTries");
            }
            set
            {
                this.Data.SetProperty<int>("UserEmailTries", value);
            }
        }
        private int UserNameTries
        {
            get
            {
                return this.Data.GetProperty<int>("UserNameTries");
            }
            set
            {
                this.Data.SetProperty<int>("UserNameTries", value);
            }
        }
        private bool IsAuthenticationRequired
        {
            get
            {
                return this.Data.GetProperty<bool>("AuthenticationRequired");
            }
            set
            {
                this.Data.SetProperty<bool>("AuthenticationRequired", value);
            }
        }

        private string AccessToken
        {
            get
            {
                return this.UserData.GetProperty<string>("AccessToken");
            }
        }

        public BotCommandHandler CommandHandler { get; private set; }
        #endregion

        private void ClearProperty<T>(string propertyName)
        {
            this.Data.SetProperty(propertyName, default(T));
        }
        private async Task ClearBotData()
        {
            await this.ClientState.BotState.DeleteStateForUserAsync(this.ChannelId, this.UserId);
        }

        public void Dispose()
        {
            if(client != null)
                client.Dispose();
        }
    }
}