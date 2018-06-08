using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIFunctionality
{
    public class QnAConnectivity
    {
        public static async Task<string> GetQnAAnswer(string question, IConfiguration configuration)
        {
            var response = "Something Happened.  Jim Bot is having issues!  Try again later.";
            try
            {

                var settings = configuration.GetSection("QnASettings");
                var endpointKey = settings["EndpointKey"];
                var knowledgeBaseId = settings["KnowledgeBaseId"];
                var url = settings["Url"];


                var qnaEndpoint1 = new QnAMakerEndpoint
                {
                    // add subscription key for QnA and knowledge base ID
                    EndpointKey = endpointKey,
                    KnowledgeBaseId = knowledgeBaseId,
                    Host = url
                };


                var qnaMaker = new Microsoft.Bot.Builder.Ai.QnA.QnAMaker(qnaEndpoint1);
                var queryResults = await qnaMaker.GetAnswers(question);
                if (queryResults != null && queryResults.Any())
                {
                    var enumerator = queryResults.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        response = $"Another possible answer is {enumerator.Current}";
                    }
                    response = queryResults.First().Answer;
                }
                else
                {
                    response = "I don't have an answer for you.";
                }
            }
            catch (Exception exception)
            {
                Console.Write(exception.Message);

                return "Error Occurred";

            }
            return response;
        }

    }
}
