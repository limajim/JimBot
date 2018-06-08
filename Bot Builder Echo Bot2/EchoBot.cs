﻿using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using AIFunctionality;
using Microsoft.Extensions.Configuration;


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
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var state = context.GetConversationState<EchoState>();
                // Bump the turn count. 
                state.TurnCount++;

                var answer = await AIBrain.GetAnswersFromQnA(context.Activity.Text, _configuration);
//                var answer = await AIBrain.GetUtteranceFromLUIS(context.Activity.Text, _configuration);
                
                // Echo back to the user whatever they typed.
                await context.SendActivity($"Turn {state.TurnCount}: '{answer}'");
            }
        }
    }
}