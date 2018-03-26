using Expense.Tracker.Data;
using Expense.Tracker.Web.Models.Bot;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Expense.Tracker.Web.Models
{
    [LuisModel("1ce4fe04-3f7e-4a0f-a055-fcd41f5048be", "8a2b0aa13fb34cec9430833e5a4b6122")]
    [Serializable]
    public class UnAuthorizedBotModel: LuisDialog<object>
    {
        public UnAuthorizedBotModel(string accessToken, string userName, string useremail, BotData userData, BotCommandHandler commandHandler, DataManager manager)
        {
            this.UserName = userName;
            this.UserEmail = useremail;
            this.AccessToken = accessToken;
            this.UserData = userData;
            this.DataFactory = manager; 
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
        private string UserEmail { get; set; }
        private DataManager DataFactory { get; set; } 
        private bool IsUserExists { get; set; }
        private string AccessToken { get; set; }

        public bool Snotify { get; set; }
        private BotCommandHandler CommandHandler { get; set; }

        [LuisIntent("features")]
        public async System.Threading.Tasks.Task Features(IDialogContext context, LuisResult result)
        {
            var firstentity = result.Entities.FirstOrDefault();

            var answer = this.CommandHandler.CheckForFeatures(result.Query, firstentity);
            if (string.IsNullOrEmpty(answer))
            {
                await context.PostAsync($"I am so sorry {this.UserName}, I think the feature you are talking about is not present or I am unaware. ");
            }
            else
                await context.PostAsync(answer);
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
                //    await context.PostAsync($"Yes we support integration with {lstEntity[0]} and {lstEntity[1]}. You can check more from [this link](http://www.appseconnect.com/integration-blueprint/)");
                //}
                //else
                    await context.PostAsync($"Not yet, but {this.UserName} if you want this connector, you can drop us a mail to sales@insync.co.in to initiate development.");
            }
            else
                await context.PostAsync($"I am so sorry {this.UserName}, you need to ensure you at least specify two application to integrate");
        }

        [LuisIntent("AboutContent")]
        public async Task AboutContent(IDialogContext context, LuisResult result)
        {
            if (this.Snotify)
            {
                if (this.CommandHandler.DetectPositiveAnswer(result.Query))
                {
                    
                    await context.PostAsync($"Thank you for your enquery. I have just notified our sales personel to get in contact with you soon. Thank you");
                }
                else
                    await context.PostAsync("No problem, feel free to talk to us anytime");
                this.Snotify = false;
            }
            else
            {
                var entityRecommendation = this.GetBestIntent(result.Entities);
                var query = result.Query;
                if (entityRecommendation != null)
                {

                    var bestEntity = entityRecommendation.Entity;
                    
                    if (entityRecommendation.Type.StartsWith("pri"))
                    {
                        await context.PostAsync($"Well, we always charge depending on complexity of the project. You can go with our [our pricing](http://www.appseconnect.com/pricing/) to know more. ");
                        //this.Snotify = true;
                    }
                    else if (bestEntity.StartsWith("integration"))
                    {
                        await context.PostAsync($"Ok, we support a bunch of integrations. Try our [blueprint page](http://www.appseconnect.com/integrations/), you will find all the integrations listed there.");
                    }
                    else if (string.IsNullOrWhiteSpace(bestEntity))
                    {
                        await context.PostAsync($"Sorry {this.UserName}, I am not a superhero and there are things that I dont understand, please ask something else or reframe your sentence so that I can answer.");
                    }

                    else
                    {
                        var answer = string.Empty; //CommonAnswers.GetItem(bestEntity, this.Store);
                        if (string.IsNullOrWhiteSpace(answer))
                        {
                            // var searchResult = this.MakeRequest(bestEntity);
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
                }
                else
                {
                    var answerfromWords = this.CheckForWords(query);
                    if (string.IsNullOrWhiteSpace(answerfromWords))
                        await context.PostAsync($"I am so sorry {this.UserName}, I am unable to answer at this moment. But I would encourage you to [click here](http://www.bing.com/search?q={query}) to know about it.");
                    else
                        await context.PostAsync(answerfromWords);
                }
            }
        }

        private string CheckForWords(string query)
        {
            return this.CommandHandler.ParseFormalWords(query);
        }

        private async Task AskUsertoRespond(IDialogContext context, IAwaitable<bool> argument)
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
                this.RemoveProperty(this.UserData.Data, "UnresolvedEntity");
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

        private void RemoveProperty(object jdata, string property)
        {
            if (jdata == null)
                jdata = new JObject();

            ((JObject)jdata).Remove(property);
        }
        private void RemoveProperty(BotData jdata, string property)
        {
            this.CommandHandler.RemoveProperty(jdata, property);
        }
        private EntityRecommendation GetBestIntent(IList<EntityRecommendation> entities)
        {
            if (entities != null && entities.Count > 0)
            {
                return entities[0];
            }
            return null;
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
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        [LuisIntent("")]
        public async System.Threading.Tasks.Task None(IDialogContext context, LuisResult result)
        {
            string message = $"I am not a superhero {this.UserName} and sometimes there are things which I don't know. Can you reframe the sentence and try again for me please.\n Or else, you can check [faqs](http://www.appseconnect.com/faqs/) too.";

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
            string message = $"What do you think? Actually I am a proprietary of InSync and built to help you regarding APPSeCONNECT. I am very excited to get in touch with you and willing to help you as much as I can. ";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Name")]
        public async System.Threading.Tasks.Task Name(IDialogContext context, LuisResult result)
        {
            string message = $"Oh, I am honored to be part of APPSeCONNECT and hence I am named as **APPSeCONNECT BOT**.";

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