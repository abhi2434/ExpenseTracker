using Expense.Tracker.Web.Models;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Expense.Tracker.Web.Controllers.BOT
{ 
    // Only enable authentication once you've configured your bot in the Bot Framework Portal and 
    // have set the BotId, MicrosoftAppId and MicrosoftAppPassword in the Web.Config for your bot 
    [BotAuthentication]
    // Prefix route for your calling controller
    [RoutePrefix("api/calling")]
    public class CallingController : ApiController
    {
        public CallingController()
            : base()
        {
            CallingConversation.RegisterCallingBot(c => new CallingModel(c));
        }

        /// <summary>
        /// This is the callback url for adec
        /// </summary>
        /// <returns></returns>
        [Route("callback")]
        public async Task<HttpResponseMessage> ProcessCallingEventAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.CallingEvent);
        }

        /// <summary>
        /// Calling bot will process incoming calls here.
        /// </summary>
        /// <returns></returns>
        [Route("call")]
        public async Task<HttpResponseMessage> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.IncomingCall);
        }
    }
}
