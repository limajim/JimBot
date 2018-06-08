using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace jamesbotv4
{
    public class LuisDispatchBot : IBot
    {
        private QnAMakerEndpoint qnaEndpoint1;
//        private QnAMakerEndpoint qnaEndpoint2;
        private Dictionary<string, QnAMakerEndpoint> qnaMap = new Dictionary<string, QnAMakerEndpoint>();

        public LuisDispatchBot(IConfiguration configuration)
        {
            var qnaUrl = "https://jvdqna.azurewebsites.net/qnamaker/knowledgebases";
            var subscriptionKey = "394ca826-fc83-47c5-b6b9-c445eef41494";
                
            var kb1 = "1d510f66 - 435b - 4fea - a8fb - b5ddc2e2c37c";
            //           var (kb1, kb2, subscriptionKey, qnaUrl) = Startup.GetQnAMakerConfig(configuration);
            this.qnaEndpoint1 = new QnAMakerEndpoint
            {
                // add subscription key for QnA and knowledge base ID
                EndpointKey = subscriptionKey,
                KnowledgeBaseId = kb1,
                Host = qnaUrl
            };
            //this.qnaEndpoint2 = new QnAMakerEndpoint
            //{
            //    // add subscription key for QnA and knowledge base ID
            //    EndpointKey = subscriptionKey,
            //    KnowledgeBaseId = kb2,
            //    Host = qnaUrl
            //};

            qnaMap.Add("MyBotQnA", qnaEndpoint1);
//            qnaMap.Add("MyBotQnA2", qnaEndpoint2);
        }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (context.Activity.MembersAdded.FirstOrDefault()?.Id == context.Activity.Recipient.Id)
                    {
                        Activity activity = new Activity();
                        await context.SendActivity("Luis Dispatch Bot says hello, you can ask me some stuff.", "James Please");
                    }
                    break;

                case ActivityTypes.Message:
                    string utterance = context.Activity.Text;
                    await context.SendActivity("You said: " + utterance);
                    var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                    var (topIntent, score) = luisResult.GetTopScoringIntent();

                    if (topIntent == "None")
                    {
                        await context.SendActivity("LUIS has no idea what you're talking about...");
                        //var image = new Attachment { ContentUrl = "C:\\Work\\jamesbotv4\\jamesbotv4\\wwwroot\\james.jpg", ContentType = "image/png" };
                        //var activity = MessageFactory.Attachment(image);
                        //await context.SendActivity(activity);

                    }
                    else
                    {
                        await context.SendActivity($"I talked to LUIS and she thinks your answer will be in the {topIntent} knowledge base.");
                        await context.SendActivity($"She gave a score of {score}");

                        if (true)
                        {
                            foreach (var intent in luisResult.Intents)
                            {
                                if (topIntent != intent.Key)
                                {
                                    await context.SendActivity($"The {intent.Key} KB is another possibility with a score of {intent.Value.Value<string>("score")}.");
                                }
                            }
                        }

                        await context.SendActivity("Now I'm going to QnA Maker!");
                        var qnaMaker = new Microsoft.Bot.Builder.Ai.QnA.QnAMaker(qnaMap[topIntent]);
                        var queryResults = await qnaMaker.GetAnswers(utterance);
                        if (queryResults != null && queryResults.Any())
                        {
                            var enumerator = queryResults.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                await context.SendActivity($"Another possible answer is {enumerator.Current}");
                            }
                            await context.SendActivity(queryResults.First().Answer);
                        }
                        else
                        {
                            await context.SendActivity("I don't have an answer for you.");
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
