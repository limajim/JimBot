using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using AIFunctionality;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot_Builder_Echo_Bot2
{
    public class EchoBot : IBot
    {
        IConfiguration _configuration;

        public EchoBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext context)
        {
            var state = context.GetConversationState<Dictionary<string, object>>();
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                   var theBrain = new AIBrain(_configuration, context);
                    //                var answer = await AIBrain.GetAnswersFromQnA(context.Activity.Text, _configuration);
                    //                var answer = await AIBrain.GetUtteranceFromLUIS(context.Activity.Text, _configuration);
                    var answer = await theBrain.CheckLUISandQandAAndGetMostAccurateResult();
                    if( !string.IsNullOrEmpty(answer))
                        await context.SendActivity($"The answer is: '{answer}'");
                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to Jim Bot 1.0.");
                        }
                    }
                    break;
            }
        }
    }
}
