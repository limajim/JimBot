using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace AIFunctionality
{
    public class LuisConnectivity
    {
        public static async Task<string> GetLUISResponse(string question, IConfiguration configuration)
        {
            var luisSettings = configuration.GetSection("LUISSettings");
            var modelId = luisSettings["ModelId"];
            var subscriptionKey = luisSettings["SubscriptionKey"];
            var url = luisSettings["Url"];
            var luisModel =
           new LuisModel(modelId,subscriptionKey, new System.Uri(url));

            LuisRecognizer luisRecognizer1;
            luisRecognizer1 = new LuisRecognizer(luisModel);
            RecognizerResult recognizerResult = await luisRecognizer1.Recognize(question, System.Threading.CancellationToken.None);

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

    }
}