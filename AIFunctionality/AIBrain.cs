using Microsoft.Extensions.Configuration;
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
            
            return await LuisConnectivity.GetLUISResponse(question, configuration);
        }
    }
}
