using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIFunctionality
{
    /// <summary>
    /// The brain will call the appropriate AI functions to get an appropriate answer.
    /// </summary>
    public class AIBrain
    {

        public static async Task<string> GetAnswersFromQnA(string question, IConfiguration configuration)
        {
            return await QnAConnectivity.GetQnAAnswer(question, configuration);
        }
        public static async Task<string> GetUtteranceFromLUIS(string question, IConfiguration configuration)
        {
            return await LuisConnectivity.GetAllIntents(question, configuration);
        }

        public static async Task<string> CheckLUISandQandAAndGetMostAccurateResult(string question, IConfiguration configuration)
        {
            var topIntent = await LuisConnectivity.GetTopIntent(question, configuration);
            var response = await GetResponseForUser(topIntent, question, configuration);
            return response;
        }

        private static async Task<string> GetResponseForUser((string intent, double score)? topIntent, string question, IConfiguration configuration)
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
                        returnValue = await GetAnswersFromQnA(question, configuration);
                        break;
                    default:
                        // The intent didn't match any case, so just display the recognition results.
                        returnValue = $"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).";
                        break;
                }
            }
            return returnValue;
        }
    }
}
