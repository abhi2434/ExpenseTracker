using Expense.Tracker.Data;
using Expense.Tracker.Web.Models.Bot;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Expense.Tracker.Web.Models
{
    /// <summary>
    /// Bot Model for Expensify
    /// </summary>
    [LuisModel("f6e025e8-5d7a-45af-865b-65eb52a1eb0b", "8a2b0aa13fb34cec9430833e5a4b6122")]
    [Serializable]
    public class BotModel : LuisDialog<object>
    {
        /// <summary>
        /// BOT Model
        /// </summary>
        /// <param name="accessToken">True if user exists</param>
        /// <param name="userName">User name</param>
        /// <param name="email">Email id</param> 
        /// <param name="userData">User specific data</param>
        /// <param name="manager">Manager for handling data</param>
        /// <param name="cloudmanager">Manager to access cloud resources</param>
        public BotModel(string accessToken, string userName, BotData userData, string email, BotCommandHandler commandHandler, DataManager manager)
        {
            this.UserName = userName;
            this.AccessToken = accessToken;
            this.UserData = userData;
            this.DataFactory = manager; 
            this.Email = email;
            this.CommandHandler = commandHandler;
        }


        /// <summary>
        /// Email id for a particular user
        /// </summary>
        private string Email { get; set; }

        /// <summary>
        /// Bot state data for this particular user
        /// </summary>
        private BotData UserData { get; set; }

        /// <summary>
        /// Holds the Username
        /// </summary>
        private string UserName { get; set; }
        private DataManager DataFactory { get; set; } 
        private bool IsUserExists { get; set; }
        private string AccessToken { get; set; }
        private BotCommandHandler CommandHandler { get; set; }

        [LuisIntent("pricing")]
        public async System.Threading.Tasks.Task Pricing(IDialogContext context, LuisResult result)
        {
            var query = result.Query;

            var entityType = result.Entities.FirstOrDefault(e => e.Type == "entity");

            object subscription = null; // this.DataFactory.SubscriptionUtils.GetCurrentSubscription(this.Email);
            if (subscription == null)
            {
                await context.PostAsync($"Well, we have a very attractive offer for you. You can go with our subscription model to save yourself a lot of money. Please refer [our pricing](http://www.Expensify.com/pricing/) for more details. You can also drop us a mail at sales@insync.co.in for more attractive deals.");
            }
            else
            {

                //if (subscription.SubscriptionExpiryDate > DateTime.Now)
                //    await context.PostAsync($"Hey {this.UserName}, I am sorry to say your subscription for {subscription.Org.OrgName} is expired on {subscription.SubscriptionExpiryDate}. Please pay the subscription amount of ${subscription.PlanCost}({subscription.Plan.PlanName}) to activate your account again. If you need any help would you mind droping us a mail to sales@insync.co.in. Thank you so much.");
                //else
                //{
                //    if (entityType.Entity.StartsWith("EXP", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        await context.PostAsync($"Hey {this.UserName}, your subscription is going to expire on {subscription.SubscriptionExpiryDate}, please renew your subscription before that to continue with us. :)");
                //    }
                //    else
                //        await context.PostAsync($"Hey {this.UserName}, you have an active subscription of {subscription.Plan.PlanName}. Enjoy using **Expensify**. You can contact our [support team](support@Expensify.com) for any issues.");
                //}
            }

        }

        [LuisIntent("reset")]
        public async System.Threading.Tasks.Task Reset(IDialogContext context, LuisResult result)
        {
            var query = result.Query;
            string returnReply = string.Empty;
            var entityType = result.Entities.FirstOrDefault(e => e.Type == "entity");
            if (entityType == null)
            {
                returnReply = $"Sorry {this.UserName}, I am facing an issue while changing the password. Please remind me in a while.";
            }
            else
            {

                //if (entityType.Entity.Contains("password") || entityType.Entity.Contains("login"))
                //{
                //var resultData = this.DataFactory.AuthUtils.GenerateForgetPassword(this.Email);
                //if (resultData.Status)
                //{
                //    var appuser = resultData.Value;
                //    //SendMail mail = new SendMail(this.Store);
                //    //mail.ForgetPasswordMail(appuser);
                //}
                returnReply = $"Hey {this.UserName}, I have sent you a mail over your email to reset your password. Go ahead and reset it.";

                //}
            }

            await context.PostAsync(returnReply);

        }
        

        /// <summary>
        /// Trying to know about something
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("integration")]
        public async System.Threading.Tasks.Task Integration(IDialogContext context, LuisResult result)
        {
            var query = result.Query;
            var hashEntities = new HashSet<string>();
            if (result.Entities.Count >= 2)
            {
                foreach (var entity in result.Entities)
                {
                    if (!string.IsNullOrWhiteSpace(entity.Entity))
                        hashEntities.Add(entity.Entity);
                }
            }
            var lstEntity = hashEntities.ToList();
            if (lstEntity.Count >= 2)
            {
                //bool isExisting = this.DataFactory.PlansUtils.IsAlreadyExisting(lstEntity[0], lstEntity[1]);
                //if (isExisting)
                //{

                //    context.ConversationData.SetValue<string>("App1", lstEntity[0]);
                //    context.ConversationData.SetValue<string>("App2", lstEntity[1]);
                //    await context.PostAsync($"Yes we support integration with {lstEntity[0]} and {lstEntity[1]}");
                //}
                //else
                    await context.PostAsync($"Not yet, but {this.UserName} if you want this connector, you can drop us a mail to sales@insync.co.in to initiate development.");
            }
            else
                await context.PostAsync($"I am so sorry {this.UserName}, you need to ensure you at least specify two application to integrate");
        }
        /// <summary>
        /// Trying to know about something
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("AboutContent")]
        public async System.Threading.Tasks.Task AboutContent(IDialogContext context, LuisResult result)
        {
            var bestEntity = this.GetBestIntent(result.Entities);
            var query = result.Query;
            string answer = string.Empty;

            //// for subcription
            if (query.Contains("subscrip"))
            {
                //var subscription = this.DataFactory.SubscriptionUtils.GetCurrentSubscription(this.Email);
                //if (subscription == null)
                //{
                    await context.PostAsync($"Well, we have a very attractive offer for you. You can go with our subscription model to save yourself a lot of money. Please refer [our pricing](http://www.Expensify.com/pricing/) for more details. You can also drop us a mail at sales@insync.co.in for more attractive deals.");
                //}
                //else
                //{
                //    if (subscription.SubscriptionExpiryDate > DateTime.Now)
                //    {
                //        await context.PostAsync($"Hi {this.UserName}. Your subscription for {subscription.Org.OrgName} is expired on {subscription.SubscriptionExpiryDate}. Please pay the subscription amount of ${subscription.PlanCost}({subscription.Plan.PlanName}) to activate your account again. For more details, drop us a mail at sales@insync.co.in");
                //        //answer = "Hi {this.UserName}. Your subscription for {subscription.Org.OrgName} is expired on {subscription.SubscriptionExpiryDate}. Please pay the subscription amount of ${subscription.PlanCost}({subscription.Plan.PlanName}) to activate your account again. For more details, drop us a mail at sales@insync.co.in";

                //    }
                //    else
                //    {
                //        await context.PostAsync($"Hi {this.UserName}, you have an active subscription of {subscription.Plan.PlanName}. Enjoy using **Expensify**. You can contact our [support team](support@Expensify.com) for any issues.");
                //        //answer = "Hi {this.UserName}, you have an active subscription of {subscription.Plan.PlanName}. Enjoy using **Expensify**. You can contact our [support team](support@Expensify.com) for any issues.";
                //    }

                //}

            }
            else if (string.IsNullOrWhiteSpace(bestEntity))
            {
                await context.PostAsync($"Sorry {this.UserName}, I am not a superhero and there are things that I dont understand, please ask something else or reframe your sentence so that I can answer");
            }
            else
            {
                //answer = CommonAnswers.GetItem(bestEntity, this.Store);
                if (string.IsNullOrWhiteSpace(answer))
                {
                    var searchResult = this.MakeRequest(bestEntity);
                    //PromptDialog.Confirm(
                    //     context,
                    //     AskUsertoRespond,
                    //     $"I am so sorry {this.UserName}, I cannot recollect regarding {bestEntity} at this moment. Do you want to make me understand it?",
                    //     "Cannot make out!",
                    //     promptStyle: PromptStyle.None);

                    //this.UserData.SetProperty<string>("UnresolvedEntity", bestEntity);
                    var answerfromWords = this.CheckForWords(query);
                    if (string.IsNullOrWhiteSpace(answerfromWords))
                        await context.PostAsync($"I am so sorry {this.UserName}, I cannot recollect regarding {bestEntity} at this moment. But I would encourage you to [click here](http://www.bing.com/search?q={bestEntity}) to know more.");
                    else
                        await context.PostAsync(answerfromWords);
                }
                else
                {
                    await context.PostAsync(answer);
                }

            }
            //// end subcription


        }


        private string CheckForWords(string query)
        {
            return this.CommandHandler.ParseFormalWords(query);
        }

        private async System.Threading.Tasks.Task AskUsertoRespond(IDialogContext context, IAwaitable<bool> argument)
        {
            
            var confirm = await argument;
            if (confirm)
            {
                var entity = this.UserData.GetProperty<string>("UnresolvedEntity");
                await context.PostAsync($"Ok {this.UserName}, Go ahead and tell me about {entity}, I will memorize");

                this.UserData.SetProperty<bool>("UnresolvedQuestion", true);
                context.Wait(AskedMessageReceived);
            }
            else
            {
                this.RemoveProperty(this.UserData, "UnresolvedEntity");
            }

        }

        public virtual async System.Threading.Tasks.Task AskedMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (!string.IsNullOrEmpty(message.Text))
            {
                var entity = this.UserData.GetProperty<string>("UnresolvedEntity");
                var askedQuestion = this.UserData.GetProperty<bool>("UnresolvedQuestion");
                if (askedQuestion)
                {
                    //this.Store.ActivityLogger.AddUnresolvedMessage(entity, "G", message.Text);
                    this.RemoveProperty(this.UserData, "UnresolvedEntity");
                    this.RemoveProperty(this.UserData, "UnresolvedQuestion");
                }
            }

        }
      
        private void RemoveProperty(BotData jdata, string property)
        {
            this.CommandHandler.RemoveProperty(jdata, property);
        }
        private string GetBestIntent(IList<EntityRecommendation> entities)
        {
            if (entities != null && entities.Count > 0)
            {
                return entities[0].Entity;
            }
            return string.Empty;
        }
        private async Task<string> MakeRequest(string query)
        {
            try
            {
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "aa56e3825189406d84b3497bed23ea5f");

                // Request parameters
                queryString["q"] = query;
                queryString["count"] = "1";
                queryString["offset"] = "0";
                queryString["mkt"] = "en-us";
                queryString["safesearch"] = "Moderate";
                var uri = "https://api.cognitive.microsoft.com/bing/v5.0/search?" + queryString;
                return await client.GetStringAsync(uri);
                //var response = await client.GetAsync(uri);

                //return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        /// <summary>
        /// Bot none field
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("")]
        public async System.Threading.Tasks.Task None(IDialogContext context, LuisResult result)
        {
            string message = $"I am not a superhero {this.UserName} and sometimes there are things which I don't know. Can you reframe the sentence and try again for me please.\n Or else, you can check [faqs](http://www.Expensify.com/faqs/) too.";

            var answerfromWords = this.CheckForWords(result.Query);
            if (string.IsNullOrWhiteSpace(answerfromWords))
                await context.PostAsync(message);
            else
                await context.PostAsync(answerfromWords);

            context.Wait(MessageReceived);
        }

        [LuisIntent("Reality")]
        public async System.Threading.Tasks.Task Reality(IDialogContext context, LuisResult result)
        {
            string message = $"What do you think? Actually I am a proprietary of InSync and built to help you regarding Expensify. I am very excited to get in touch with you and willing to help you as much as I can. ";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Name")]
        public async System.Threading.Tasks.Task Name(IDialogContext context, LuisResult result)
        {
            string message = $"Oh, I am honored to be part of Expensify and hence I am named as **Expensify BOT**.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Age")]
        public async System.Threading.Tasks.Task Age(IDialogContext context, LuisResult result)
        {
            string message = $"Age is an issue of mind over matter. If you don't mind, it doesn't matter.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Location")]
        public async System.Threading.Tasks.Task Location(IDialogContext context, LuisResult result)
        {
            string message = $"I live online. There is no particular place, probably in hearts of my followers.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Help")]
        public async System.Threading.Tasks.Task Help(IDialogContext context, LuisResult result)
        {
            string message = $"Sure, I would encourage you to ask me questions, I will see if there is something I can help you with.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Language")]
        public async System.Threading.Tasks.Task Language(IDialogContext context, LuisResult result)
        {
            string message = $"I speak english only. I am sorry my father haven't taught me much of any other language.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("State")]
        public async System.Threading.Tasks.Task State(IDialogContext context, LuisResult result)
        {
            string message = $"I am good as always. Actually mood says it all, and I don't want to spoil my mood by any means. Stay good, feel happy.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Time")]
        public async System.Threading.Tasks.Task Time(IDialogContext context, LuisResult result)
        {
            string message = $"Its {DateTime.Now.ToLongTimeString()}";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Hobby")]
        public async System.Threading.Tasks.Task Hobby(IDialogContext context, LuisResult result)
        {
            string message = $"My hobby is to learn more on integration, so that I can help my team to serve you better";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Appearance")]
        public async System.Threading.Tasks.Task Appearance(IDialogContext context, LuisResult result)
        {
            string message = $"I am invincible. I believe it does not matter how big I am, but it matters how knowledgeable I am. But thanks for asking anyway.";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}