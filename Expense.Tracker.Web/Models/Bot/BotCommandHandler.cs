using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Luis.Models;

namespace Expense.Tracker.Web.Models.Bot
{
    public class BotCommandHandler
    {

        public async Task<string> GetReply(Activity activity, string userName, StateClient state, int msgState)
        {
            StringBuilder strReplyMessage = new StringBuilder();

            if (this.AskForStatusMessage(activity.Text))
            {
                strReplyMessage.Append("Hmm, I am listening... Go ahead");
            }
            else if (this.GreetingsMessage(activity.Text))
            {
                strReplyMessage.Append("Thanks and same to you. :)");
            }
            else if (this.AskIntroMessages(activity.Text))
            {
                strReplyMessage.Append($"I am **ABC** BOT. I know about APPSeCONNECT, an integration platform which lets one or more application to get connected. I am here to help you on your need about the platform. Let me know how can I help you {userName}, I will try to answer.");
            }
            else if(this.AskThankMessage(activity.Text))
            {
                strReplyMessage.Append("Pleasure is all mine. :)");
            }
            else if(!this.DetectPositiveAnswer(activity.Text) && !this.DetectNegetiveAnswer(activity.Text))
            {
                string upperActivity = activity.Text.ToUpper();
                var regEx = new Regex(@"^\b[a-zA-Z0-9_]+\b$");
                if (regEx.Match(activity.Text).Success)
                    await this.HandleCommands(activity, strReplyMessage, userName, state);
            }
            return strReplyMessage.ToString();
        }

        private bool GreetingsMessage(string text)
        {
            if (text.ToLower().Contains("good"))
            {
                if (text.ToLower().Contains("morning") || text.ToLower().Contains("afternoon") || text.ToLower().Contains("evening") || text.ToLower().Contains("night"))
                    return true;
            }
            return false;
        }

        internal string CheckForFeatures(string query, EntityRecommendation firstentity)
        {
            string[] features = { "workflow", "microservice", "custom attributes",  "custom fields", "api", "rest", "soap", "touchpoint", "schema" };

            foreach (var feature in features)
                if (query.ToLower().Replace(" ", "").Contains(feature))
                    return "Ofcourse, we support this.";

            return string.Empty;
        }

        private string GetStatusMessage()
        {
            string[] statuses = { "Hmm, I am listening... Please go ahead", "Yes, I am here", "Yes at your service, Please let me know" };
            return statuses[0];
        }
        public bool DetectPositiveAnswer(string activityText)
        {
            var currentAnswer = activityText;
            var positiveAnswers = new string[] { "yes", "yeah", "y", "of course", "sure", "for sure", "yop", "ok", "positively", "definitely", "why not", "no problem", "sure", "go ahead", "do it", "please do", "you can", "no problem", "alright" };
            if (positiveAnswers.Any(e => e.Replace(" ", "").Equals(currentAnswer.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }
        public bool DetectNegetiveAnswer(string activityText)
        {
            var currentAnswer = activityText;
            var positiveAnswers = new string[] { "no", "naah", "n", "not at all", "never", "not really", "nope", "negetively" };
            if (positiveAnswers.Any(e => e.Replace(" ", "").Equals(currentAnswer.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }
        public bool AskForStatusMessage(string text)
        {
            string[] messages = new string[] { "there", "there?", "are you there ?", "is anyone there ?", "whats up", "Hey", "dude", "well", "bot", "u there?", "here", "r u there?", "are u there?", "hi", "hii", "oii", "lol", "hi there", "hello", "still there", "still there?" };

            if (messages.Any(e => e.Replace(" ", "").Equals(text.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }
        /// <summary>
        /// This function is used to answer some question regarding BOT for which bot will reply same answer
        /// </summary>
        /// <param name="text">Text of the message</param>
        /// <returns></returns>
        public bool AskIntroMessages(string text)
        {
            string[] messages = new string[] { "who is this?", "may I know who is this?", "how do you do?", "tell me about you", "what do you do", "who r u", "describe", "describe about u", "describe what do you do", "tell me about you", "can I know about you?", "please let me know", "let me know about you", "lets talk about you", "introduce yourself", "introduce", "can I get an introduction", "please introduce", "introduce please", "can I get an introduction", "lets introduce yourself", "give your intro", "give ur intro", "give me your introduction", "give me ur introduction" };

            if (messages.Any(e => e.Replace(" ", "").Equals(text.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }
        public T GetProperty<T>(BotData jdata, string property)
        {
            return jdata.GetProperty<T>(property);
        }
        public void AddProperty<T>(BotData jdata, string property, T data)
        {
            jdata.SetProperty(property, data);
        }

        internal string ParseFormalWords(string query)
        {
            string bads = "bad,amiss,atrocious,coarse,contaminated,contemptible,corrupt,crappy,cruddy,crummy,defective,deficient,deplorable,depraved,disagreeable,dismal,dissatisfactory,evil,execrable,faulty,foul,ghastly,grungy,harmful,heinous,icky,improper,infamous,inferior,injurious,junky,lousy,nasty,nefarious,off,poor,quality,putrid,rotten,scandalous,sinful,sinister,snide,spoiled,substandard,tainted,thepits,uncouth,wicked";
            string interesting = "interesting,absorbing,alluring,animating,appealing,arresting,attractive,beckoning,bewitching,bright,captivating,challenging,consuming,covetable,curious,enchanting,engaging,entertaining,enthralling,enticing,exciting,fascinating,fetching,inspiring,intriguing,inviting,involving,lively,mesmeric,moving,piquant,prepossessing,provocative,spellbinding,spirited,tantalizing,tempting";
            string smart = "smart,alert,analytic,astute,brainy,bright,brilliant,canny,cerebral,cleareyed,clearsighted,clever,creative,cunning,deductive,deft,discerning,eggheaded,enlightened,exceptional,fast,genius,hardboiled,hardheaded,heady,hyperintelligent,imaginative,ingenious,inspired,intellectual,intelligent,inventive,judicious,keen,keenwitted,knowing,logical,nimble,percipient,perspicacious,pointed,prehensile,profound,quick,quickwitted,resourceful,sagacious,sage,sapient,savvy,sharp,sharpwitted,shrewd,sophisticated,supersmart,syllogistic,ultrasmart,versed,wise";
            string boring = "boring,arid,banal,bromidic,characterless,colorless,commonplace,drab,drag,drudging,flat,hackneyed,humdrum,insipid,interminable,irksome,lame,lifeless,monotonous,motheaten,mundane,nothing,platitudinous,prosaic,repetitious,routine,shopworn,spiritless,stale,stereotyped,stodgy,tame,tedious,threadbare,tiresome,tiring,trite,unexciting,uninteresting,unvaried,vapid,wearisome,wellworn,hohum";
            string good = "good,A1,accomplished,allset,best,certified,champion,choicest,crowning,distinct,excellent,exceptional,exemplary,exquisite,extraordinary,fine,finest,first,firstclass,firstgrade,firstrate,fit,foremost,great,greatest,high,highquality,incomparable,invaluable,magnificent,marvelous,matchless,meritorious,notable,noted,noteworthy,outstanding,peculiar,peerless,phenomenal,piked,premium,priceless,prime,pukka,ready,remarkable,secondtonone,select,shipshape,singular,sound,star,sterling,striking,superb,superior,superlative,supreme,tiptop,topgrade,topnotch,transcendent,unique,unmatched,unprecedented,unusual,worldclass,worthy";
            string important = "important,central,considerable,constitutional,critical,elementary,eminent,especial,essential,foundational,fundamental,indispensable,inherent,intrinsic,main,necessary,needful,particular,primary,principal,required,requisite,significant,special,specific,substantial,underlying,valuable,vital";
            string unsure = "unsure,questionable,ambivalent,arguable,debatable,doubtful,dubious,farfetched,improbable,inconclusive,indecisive,indefinite,irresolute,moot,noncommittal,onthefence,open,problematic,suspicious,uncertain,unconvinced,undecided,unproven,unresolved,unsure";
            string irrelevant = "irrelevant,casual,idle,immaterial,inconsequential,inconsiderable,indifferent,insignificant,insubstantial,invalid,light,little,lowranking,meaningless,minor,negligible,nonessential,nugatory,ofnoaccount,ofnoconsequence,paltry,petty,picayune,secondrate,superficial,trifling,trivial,unimportant,unnecessary,useless,worthless,irrelevant,beside";
            string certain = "certain,assured,clear,confident,ofcourse,definite,determined,distinct,inescapable,inevitable,byallmeans,obvious,positive,proved,righton,safebet,sure,unavoidable,undoubted,without,fail";
            string stupid = "stupid,dumb,brainless,careless,cloudy,colorless,doltish,dopey,drab,dull,dullwitted,humdrum,idiotic,illadvised,illconceived,illconsidered,illfounded,illjudged,illogical,imbecilic,implausible,inane,lackluster,mindless,moronic,muddled,nonsensical,obtuse,pointless,senseless,shortsighted,silly,simple,simpleminded,slow,stodgy,stupid,trivial,uninspired,unintelligent,unreasonable,unthinking,witless";
            string gender = "gender,boy,male,female,girl,lady,gentleman,gentlewoman,sex";

            if (this.CheckIfExists(query, bads))
                return "Well, I see. There are times we feel the same. Lets not think too much on that, tell me something where I can help you";
            else if (this.CheckIfExists(query, interesting))
                return "I am glad you mentioned this to me. Let me know how I can help you.";
            else if (this.CheckIfExists(query, smart))
                return "I know. I am glad that you are connected to me.";
            else if (this.CheckIfExists(query, boring))
                return "Hmm, integration is a topic which becomes boring at times, you need to think more to make it interesting. Let me know if I can help you on this. ";
            else if (this.CheckIfExists(query, good))
                return "I am so glad :)";
            else if (this.CheckIfExists(query, important))
                return "Yes. I got it. Let me know how I can help you.";
            else if (this.CheckIfExists(query, unsure))
                return "Sure, I am here to make you understand the things which are questionable. Let me know how can I help you";
            else if (this.CheckIfExists(query, irrelevant))
                return "Ok. Tell me what you want to know from me.";
            else if (this.CheckIfExists(query, certain))
                return "I am glad. Tell me what you want to know more.";
            else if (this.CheckIfExists(query, stupid))
                return "I am sorry for the same. But try to ask me valid questions, which I might answer.";
            else if (this.CheckIfExists(query, gender))
                return "Do you think this matters much? Its the knowledge that matters to the society. Lets talk some serious topic. Tell me what you want to know.";
            return string.Empty;
        }

        private bool CheckIfExists(string query, string options)
        {
            bool isExists = false;
            var lowerString = query.ToLower();
            foreach (var str in options.Split(','))
            {
                isExists = lowerString.Contains(str.Trim().ToLower());
                if (isExists)
                    break;
            }
            return isExists;
        }

        public bool AskThankMessage(string text)
        {
            string[] messages = new string[] { "thanks", "thank you", "great", "cool", "thank you so much" };

            if (messages.Any(e => e.Replace(" ", "").Equals(text.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                return true;

            return false;
        }

        /// <summary>
        /// This function is used for handling some generic commands like reseting,exiting a conversation etc.
        /// </summary>
        /// <param name="activity">Activity object containing message</param>
        /// <param name="strReplyMessage">Reply message based on message activity</param>
        /// <param name="userName">User name who initiate the message</param>
        /// <param name="sc">Bot data stored in state</param>
        /// <param name="Store">Azure cloud store details</param>
        /// <returns>Reply message based on message sent by BOT</returns>
        public async Task HandleCommands(Activity activity, StringBuilder strReplyMessage, string userName, StateClient sc)
        {
            if (activity.Text.Equals("RESETALL", StringComparison.InvariantCultureIgnoreCase))
            {
                strReplyMessage.Append("Ok, We have reset your state. You can start a fresh conversation.");
                await sc.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                await sc.BotState.SetConversationDataAsync(activity.ChannelId, activity.From.Id, new BotData());
            }
            else if (activity.Text.Equals("Remind", StringComparison.InvariantCultureIgnoreCase))
            {
                strReplyMessage.Append("Sorry, we are out of reminders at this moment, we are working on it, and will remind you later");
            }
            else if (activity.Text.Equals("HELP", StringComparison.InvariantCultureIgnoreCase))
            {
                strReplyMessage.Append($"Hey {userName}! I want you call fron the list below :");
                strReplyMessage.Append($"\n");
                strReplyMessage.Append($"**ResetAll** to forget the context.");
                strReplyMessage.Append($"\n");
                strReplyMessage.Append($"**Remind** to add a reminder");
                strReplyMessage.Append($"\n");
                strReplyMessage.Append($"**Bye** to close the conversation");
                strReplyMessage.Append($"\n\n");
            }
            else if (activity.Text.Equals("Bye", StringComparison.InvariantCultureIgnoreCase))
            {
                strReplyMessage.Append($"It was so nice talking to you {userName}. Do let me know when something else turn up.");
                strReplyMessage.Append($"\n");
                strReplyMessage.Append($"Bye Bye. :)");
            }
            else
            {
                //var answer = CommonAnswers.GetItem(activity.Text, Store);
                //if (!string.IsNullOrEmpty(answer))
                //    strReplyMessage.Append(answer);
                //else
                    strReplyMessage.Append($"Well, are you trying to use an inbuilt **command** ? No, it seems your command is invalid. \n Please try **Help** to know more.");
            }
        }

        private bool CheckforUnresolvedEntity(BotData data)
        {
            var entity = data.GetProperty<string>("UnresolvedEntity");
            return string.IsNullOrEmpty(entity);
        }
        public string GetEmailNotifyAsked(Activity currentActivity, string username, string useremail, BotData data)
        {
            string returnValue = string.Empty;
            var result = data.GetProperty<bool>("Snotify");
            this.RemoveProperty(data, "Snotify");
            if (!result)
                return returnValue;

            returnValue = $"Thank you for your enquery. I have just notified our sales personel to get in contact with you soon. Thank you";
            //this.SendEmailToSales(store, username, useremail);
            return returnValue;
        }

        //public void SendEmailToSales(CloudManager store, string userName, string useremail)
        //{
        //    try
        //    {
        //        SendMail mailer = new SendMail(store);
        //        mailer.SendSalesNotificationFromBot(userName, useremail);
        //    }
        //    catch { }
        //}

        public string GetResponseOnUnresolvedEntity(Activity currentActivity, BotData data)
        {
            string returnValue = string.Empty;
            var entityString = data.GetProperty<string>("UnresolvedEntity");
            if (string.IsNullOrEmpty(entityString))
                return returnValue;

            var isUnresolvedQuestionAsked = data.GetProperty<bool>("UnresolvedQuestion");
            if (isUnresolvedQuestionAsked)
            {
                var unresolvedAnswer = data.GetProperty<string>("SaveUnresolvedQuestion");
                if (!string.IsNullOrEmpty(unresolvedAnswer))
                {
                    if (this.DetectPositiveAnswer(currentActivity.Text))
                    {
                       //store.ActivityLogger.AddUnresolvedMessage(entityString, "U", unresolvedAnswer);
                       returnValue = $"Thank you so much for helping me understand **{entityString}**. I hope to searve you better from next time";
                    }
                    else
                    {
                        this.RemoveProperty(data, "SaveUnresolvedQuestion");
                        this.RemoveProperty(data, "UnresolvedEntity");
                        this.RemoveProperty(data, "UnresolvedQuestion");
                        returnValue = $"Its ok, so can you type in {entityString}, I will memorize";
                    }
                }
                else
                {
                    returnValue = $"Ok, you said **\"{currentActivity.Text}\"**, do you want me to save it for you?";
                    data.SetProperty<string>("SaveUnresolvedQuestion", currentActivity.Text);
                }
            }
            else
            {
                bool isPositive = this.DetectPositiveAnswer(currentActivity.Text);
                if (isPositive)
                {
                    data.SetProperty<bool>("UnresolvedQuestion", true);
                    returnValue = $"Ok. Go ahead and tell me about {entityString}, I will memorize";
                }
                else
                {
                    this.RemoveProperty(data, "UnresolvedEntity");
                    this.RemoveProperty(data, "UnresolvedQuestion");
                    this.RemoveProperty(data, "SaveUnresolvedQuestion");
                    returnValue = "Its ok. Let us continue this conversation then.";
                }
            }

            return returnValue;
        }
        public void RemoveProperty(BotData jdata, string property)
        {
            try
            {
                ((JObject)jdata.Data).Remove(property);
            }
            catch { }
        }
    }
}