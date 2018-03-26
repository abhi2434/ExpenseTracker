using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Expense.Tracker.Web.Models.Bot
{
    public class BotAuthenticator
    {
        
        public BotAuthenticator()
        { 
        }
        /// <summary>
        /// This function is written for showing the initial login dialog after adding the bot
        /// </summary>
        /// <param name="activity">Activity associated with a bot</param>
        /// <returns>Response of the recent activity</returns>
        public async Task ShowLoginDialog(Activity activity)
        {
            ConnectorClient loginConnector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity replyToConversation = activity.CreateReply();
            replyToConversation.Recipient = activity.From;
            replyToConversation.Type = "message";
            replyToConversation.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"{ConfigurationManager.AppSettings["AppWebSite"]}?userId={HttpUtility.UrlEncode(activity.From.Id)}&serviceUrl={HttpUtility.UrlEncode(activity.ServiceUrl)}&conversationId={activity.Conversation.Id}&channelId={HttpUtility.UrlEncode(activity.ChannelId)}",
                Type = "signin",
                Title = "Authentication Required"
            };
            cardButtons.Add(plButton);
            SigninCard plCard = new SigninCard("Please login to AEX", new List<CardAction>() { plButton });
            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);
            await loginConnector.Conversations.SendToConversationAsync(replyToConversation); 
        }
        /// <summary>
        /// This function is written for showing the initial login dialog after adding the bot
        /// </summary>
        /// <param name="activity">Activity associated with a bot</param>
        /// <returns>Response of the recent activity</returns>
        public async Task ShowSupportEnginner(Activity activity)
        {
            ConnectorClient loginConnector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity replyToConversation = activity.CreateReply();
            replyToConversation.Recipient = activity.From;
            replyToConversation.Type = "message";
            replyToConversation.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = $"skype:subhajit.insync?chat",
                Type = "openUrl",
                Title = "Subhajit Goswami"
            };
            cardButtons.Add(plButton);
            SigninCard plCard = new SigninCard("Please chat with our support", new List<CardAction>() { plButton });
            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);
            await loginConnector.Conversations.SendToConversationAsync(replyToConversation);
        }
    }
}