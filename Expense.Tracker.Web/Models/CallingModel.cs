using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using ExpenseTracker.Utilities.Azure;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Expense.Tracker.Web.Models
{
    /// <summary>
    /// Constants that define the APPSeCONNECT
    /// </summary>
    public static class IvrOptions
    {
        internal const string WelcomeMessage = "Hello, thank you for calling APPSeCONNECT Team.";
        internal const string MainMenuPrompt = "If you are a new client press 1, for technical support press 2, if you need information about our recent payment updates press 3, to hear more about the company press 4. To repeat the options press 5.";
        internal const string NewClientPrompt = "To check our latest offer press 1, to talk to a technical consultant press 2. Press the hash key to return to the main menu";
        internal const string SupportPrompt = "To check our current outages press 1, to contact the technical support consultant press 2. Press the hash key to return to the main menu";
        internal const string PaymentPrompt = "To get the payment details press 1, press 2 if your payment is not visible in the system. Press the hash key to return to the main menu";
        internal const string MoreInfoPrompt = "APPSeCONNECT is a leading integration software that is used all over the world. We provide end to end business solutions around the world where our hybrid solution integrates two or more applications and provide complex solutions for your business. ";
        internal const string NoConsultants = "Unfortunately there are no consultants available at this moment. Please leave your name, and a brief message after the signal. You can press the hash key when finished. We will call you as soon as possible.";
        internal const string Ending = "Thank you for leaving the message, goodbye";
        internal const string Offer = "You can sign up for a 30 days free trial where we will demonstrate how synchronization works in your application, or you can go for subscription model where we will implement your business process through our solution";
        internal const string CurrentOutages = "There is currently 1 outage in Indian data centers, we are working on fixing the issue";
        internal const string PaymentDetailsMessage = "You should do the wire transfer to our account number 3983815 and then either call us or send an email to support@appseconnect.com with your transaction details to activate your subscription.";
    }

    /// <summary>
    /// Calling responses for the BOT.
    /// </summary>
    public class CallingModel : ICallingBot
    {
        private const string NewClient = "1";
        private const string Support = "2";
        private const string Payments = "3";
        private const string MoreInfo = "4";
        private const string NewClientOffer = "1";
        private const string NewClientOrder = "2";
        private const string SupportOutages = "1";
        private const string SupportConsultant = "2";
        private const string PaymentDetails = "1";
        private const string PaymentNotVisible = "2";

        /// <summary>
        /// Bot service which maps the calls
        /// </summary>
        public ICallingBotService CallingBotService { get; private set; }

        /// <summary>
        /// Calling Bot model constructor
        /// </summary>
        /// <param name="callingBotService"></param>
        public CallingModel(ICallingBotService callingBotService)
        {
            if (callingBotService == null)
                throw new ArgumentNullException(nameof(callingBotService));
            CallingBotService = callingBotService;
            CallingBotService.OnIncomingCallReceived += OnIncomingCallReceived;
            CallingBotService.OnPlayPromptCompleted += OnPlayPromptCompleted;
            CallingBotService.OnRecordCompleted += OnRecordCompleted;
            CallingBotService.OnRecognizeCompleted += OnRecognizeCompleted;
            CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        private BlobStorageUtil _cloudService;
        /// <summary>
        /// Cloud service that can get hold of the blob storage
        /// </summary>
        public BlobStorageUtil CloudService
        {
            get
            {
                this._cloudService = this._cloudService ?? new BlobStorageUtil(ConfigurationManager.AppSettings["AzureStoreConnection"]);
                return this._cloudService;
            }
        }
        private class CallState
        {
            public string InitiallyChosenMenuOption { get; set; }
        }

        private readonly Dictionary<string, CallState> _callStateMap = new Dictionary<string, CallState>();

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Female };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
        private static Recognize CreateIvrOptions(string textToBeRead, int numberOfOptions, bool includeBack)
        {
            if (numberOfOptions > 9)
                throw new Exception("too many options specified");
            var id = Guid.NewGuid().ToString();
            var choices = new List<RecognitionOption>();
            for (int i = 1; i <= numberOfOptions; i++)
            {
                choices.Add(new RecognitionOption { Name = Convert.ToString(i), DtmfVariation = (char)('0' + i) });
            }
            if (includeBack)
                choices.Add(new RecognitionOption { Name = "#", DtmfVariation = '#' });
            var recognize = new Recognize
            {
                OperationId = id,
                PlayPrompt = GetPromptForText(textToBeRead),
                BargeInAllowed = true,
                Choices = choices
            };
            return recognize;
        }

        private static void SetupRecording(Workflow workflow)
        {
            //Calling another person

            var id = Guid.NewGuid().ToString();
            var prompt = GetPromptForText(IvrOptions.NoConsultants);
            var record = new Record
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 10,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 2,
                PlayBeep = true,
                StopTones = new List<char> { '#' }
            };
            workflow.Actions = new List<ActionBase> { record };
        }

        private void SetupInitialMenu(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase> { CreateIvrOptions(IvrOptions.MainMenuPrompt, 5, false) };
        }
        private Recognize CreateNewClientMenu()
        {
            return CreateIvrOptions(IvrOptions.NewClientPrompt, 2, true);
        }
        private Recognize CreateSupportMenu()
        {
            return CreateIvrOptions(IvrOptions.SupportPrompt, 2, true);
        }
        private Recognize CreatePaymentsMenu()
        {
            return CreateIvrOptions(IvrOptions.PaymentPrompt, 2, true);
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var id = Guid.NewGuid().ToString();
            _callStateMap[incomingCallEvent.IncomingCall.Id] = new CallState();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                new Answer { OperationId = id },
                GetPromptForText(IvrOptions.WelcomeMessage)
            };
            return Task.FromResult(true);
        }

        private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var callStateForClient = _callStateMap[playPromptOutcomeEvent.ConversationResult.Id];
            callStateForClient.InitiallyChosenMenuOption = null;
            SetupInitialMenu(playPromptOutcomeEvent.ResultingWorkflow);
            return Task.FromResult(true);
        }

        private Task OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
        {
            var callStateForClient = _callStateMap[recognizeOutcomeEvent.ConversationResult.Id];
            switch (callStateForClient.InitiallyChosenMenuOption)
            {
                case null:
                    ProcessMainMenuSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case NewClient:
                    ProcessNewClientSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case Support:
                    ProcessSupportSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case Payments:
                    ProcessPaymentsSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                default:
                    SetupInitialMenu(recognizeOutcomeEvent.ResultingWorkflow);
                    break;
            }
            return Task.FromResult(true);
        }
        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private void ProcessMainMenuSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                SetupInitialMenu(outcome.ResultingWorkflow);
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case NewClient:
                    callStateForClient.InitiallyChosenMenuOption = NewClient;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
                    break;
                case Support:
                    callStateForClient.InitiallyChosenMenuOption = Support;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
                    break;
                case Payments:
                    callStateForClient.InitiallyChosenMenuOption = Payments;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
                    break;
                case MoreInfo:
                    callStateForClient.InitiallyChosenMenuOption = MoreInfo;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { GetPromptForText(IvrOptions.MoreInfoPrompt) };
                    break;
                default:
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }
        private void ProcessNewClientSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case NewClientOffer:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText(IvrOptions.Offer),
                    CreateNewClientMenu()
                };
                    break;
                case NewClientOrder:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }
        private void ProcessSupportSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case SupportOutages:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText(IvrOptions.CurrentOutages),
                    CreateSupportMenu()
                };
                    break;
                case SupportConsultant:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }
        private void ProcessPaymentsSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case PaymentDetails:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText(IvrOptions.PaymentDetailsMessage),
                    CreatePaymentsMenu()
                };
                    break;
                case PaymentNotVisible:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            var id = Guid.NewGuid().ToString();
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                var container = this.CloudService.CreateContainer("advanced", BlobContainerPublicAccessType.Container);
                await container.CreateIfNotExistsAsync();

                this.CloudService.CreateCloudBlob(container, record, $"{recordOutcomeEvent.RecordOutcome.Id}.wma");
            }

            recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                GetPromptForText(IvrOptions.Ending),
                new Hangup { OperationId = id }
            };
            recordOutcomeEvent.ResultingWorkflow.Links = null;
            _callStateMap.Remove(recordOutcomeEvent.ConversationResult.Id);
        }
    }
}