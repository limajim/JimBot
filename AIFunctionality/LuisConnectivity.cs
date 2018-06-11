using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace AIFunctionality
{
    public class LuisConnectivity
    {
        public static async Task<string> GetAllIntents(string question, IConfiguration configuration)
        {
            var recognizerResult = await GetResults(question, configuration);

            var intentsList = new List<string>();
            var entitiesToReturn = "LUIS did not find anything";

            foreach (var intent in recognizerResult.Intents)
            {
                intentsList.Add($"'{intent.Key}', score {intent.Value}\n\n");
            }

            if (intentsList.Count > 0)
            {
                entitiesToReturn = ($"The following entities were found in the message:\n\n{string.Join("\n\n", intentsList.ToArray())}");
            }

            return entitiesToReturn;
        }

        public static async Task<(string intent, double score)?> GetTopIntent(string question, IConfiguration configuration)
        {
            RecognizerResult recognizerResult = await GetResults(question, configuration);
            var topIntent = recognizerResult?.GetTopScoringIntent();
            return topIntent;
        }

        private static async Task<RecognizerResult> GetResults(string question, IConfiguration configuration)
        {
            var luisSettings = configuration.GetSection("LUISSettings");
            var modelId = luisSettings["ModelId"];
            var subscriptionKey = luisSettings["SubscriptionKey"];
            var url = luisSettings["Url"];
            var luisModel = new LuisModel(modelId, subscriptionKey, new System.Uri(url));

            LuisRecognizer luisRecognizer1;
            luisRecognizer1 = new LuisRecognizer(luisModel);
            var recognizerResult = await luisRecognizer1.Recognize(question, System.Threading.CancellationToken.None);

            return recognizerResult;
        }
    }
}