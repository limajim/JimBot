using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIFunctionality
{
    /// <summary>
    /// The brain will call the appropriate AI functions to get an appropriate answer.
    /// </summary>
    public class AIBrain
    {
        private IConfiguration _configuration;
        private ITurnContext _context;
        private DialogSet AiDialogSet { get; set; }

        public AIBrain(IConfiguration configuration, ITurnContext context)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> GetAnswersFromQnA()
        {
            return await QnAConnectivity.GetQnAAnswer(_context.Activity.Text, _configuration);
        }
        public async Task<string> GetAllIntentsFromLUIS()
        {
            return await LuisConnectivity.GetAllIntents(_context.Activity.Text, _configuration);
        }

        public async Task<string> CheckLUISandQandAAndGetMostAccurateResult()
        {
            var response = string.Empty;
            var state = _context.GetConversationState<Dictionary<string, object>>();
            if (state.Count == 0)
            {
                var topIntent = await LuisConnectivity.GetTopIntent(_context.Activity.Text, _configuration);
                response = await GetResponseForUser(topIntent);
            }
            else
            {
                await RunDialogContext();
            }

            return response;
        }

        private async Task<string> GetResponseForUser((string intent, double score)? topIntent)
        {
            var returnValue = string.Empty;
            if (!topIntent.HasValue)
            {
                returnValue = "Unable to get the top intent.";
            }
            else
            {
                switch (topIntent.Value.intent.ToLowerInvariant())
                {
                    case "calendar_add":
                        returnValue = "You asked about adding a calendar item!";
                        break;
                    case "calendar_checkavailability":
                        returnValue = "You asked about checking availability on a calendar";
                        break;
                    case "calendar_delete":
                        returnValue = "You asked about deleting a calendar event";
                        break;
                    case "calendar_edit":
                        returnValue = "You asked about editing a calendar event";
                        break;
                    case "calendar_find":
                        returnValue = "You asked about finding a calendar event";
                        break;
                    case "weather_getcondition":
                        returnValue = "You asked about the condition of weather! It is always beautiful in Rochester.";
                        break;
                    case "weather_getforecast":
                        returnValue = "You asked about getting the forcast for weather! It is always sunny and pleasant in Rochester.";
                        break;
                    case "none":
                        returnValue = "I have no clue what you want";
                        break;
                    case "qnaquestion":
                        // QnA call here.
                        returnValue = await GetAnswersFromQnA();
                        break;
                    case "profilechange":
                        try
                        {
                            await RunDialogContext();
                        }
                        catch ( Exception exception )
                        {
                            returnValue = $"An Exception Occurred: {exception.Message}";
                        }
                        break;
                    default:
                        // The intent didn't match any case, so just display the recognition results.
                        returnValue = $"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).";
                        break;
                }
            }
            return returnValue;
        }

        private async Task RunDialogContext()
        {
            AiDialogSet = CreateDialogSet();
            var state = _context.GetConversationState<Dictionary<string, object>>();
            var dc = AiDialogSet.CreateContext(_context, state);
            await dc.Continue();

            if (!_context.Responded)
            {
                await dc.Begin("firstRun");
            }
        }

        private DialogSet CreateDialogSet()
        {
            var dialogs = new DialogSet();

            dialogs.Add("getProfile", new ProfileControl());
            dialogs.Add("firstRun",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                         await dc.Context.SendActivity("Welcome! We need to ask a few questions to get started.");
                         await dc.Begin("getProfile");
                    },
                    async (dc, args, next) =>
                    {
                        await dc.Context.SendActivity($"Thanks {args["name"]} I have your phone number as {args["phone"]}!");
                        await dc.End();
                    }
                }
            );
            return dialogs;
        }
    }
}
