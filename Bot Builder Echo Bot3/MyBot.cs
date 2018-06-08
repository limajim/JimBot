using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace jamesbotv4
{
    public class MyBot : IBot
    {
        private DialogSet dialogs;

        public MyBot()
        {
            if (dialogs == null)
            {
                dialogs = new DialogSet();
                dialogs.Add("initial", new WaterfallStep[]
                {
                async (dc, args, next) =>
                {
                    var convo = ConversationState<Dictionary<string, object>>.Get(dc.Context);
                    if (convo.ContainsKey("Name"))
                    {
                        await next.Invoke(args);
                    }
                    else
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("textPrompt", "Hey there ya shifty wanker!  What's your name?");
                    }
                },
                async (dc, args, next) =>
                {
                    var convo = ConversationState<Dictionary<string, object>>.Get(dc.Context);
                    if (args != null) convo["Name"] = args["Value"];

                    Choice echoBotChoice = new Choice{
                        Value = "echo_bot"
                        /*
                        Action = new CardAction
                        {
                            Type = ActionTypes.ImBack,
                            Title = "Echo Bot",
                            Text = "Echo Bot",
                            Value = "echo_bot"
                        }
                        */
                    };

                    var prompt = new ChoicePromptOptions{
                        Choices = new List<Choice>()
                        {
                            new Choice{ Value = "echo_bot" },
                            new Choice{ Value = "intent_bot" },
                            new Choice{ Value = "luis_bot" },
                            new Choice{ Value = "I'm Done Here" }
                        }
                    };

                    await dc.Prompt("choicePrompt", $"What Bot do you want to play with, {convo["Name"]}?", prompt);
                },
                async (dc, args, next) =>
                {
                    var foundChoice = (FoundChoice)args["Value"];

                    await dc.Prompt("textPrompt", $"You selected: {foundChoice.Value}");

                    switch (foundChoice.Value)
                    {
                        case "echo_bot":
                            await dc.Replace("echo", args);
                            break;

                        case "intent_bot":
                            await dc.Replace("intent", args);
                            break;

                        case "I'm Done Here":
                            await dc.Context.SendActivity("See ya!");
                            await dc.End();
                            break;

                        default:
                            await dc.Context.SendActivity("You selected something I don't understand.");
                            await dc.Replace("initial");
                            break;
                    }
                }
                });

                dialogs.Add("echo", new WaterfallStep[]
                {
                async (dc, args, next) =>
                {
                    var convo = ConversationState<Dictionary<string, object>>.Get(dc.Context);
                    if (! convo.ContainsKey("promptedOnce"))
                    {
                        convo["promptedOnce"] = true;
                        await dc.Prompt("textPrompt", $"Tell me something, {convo["Name"]}.  Say 'done' to quit.");
                    }
                    else
                    {
                        await dc.Prompt("textPrompt", "Tell me something else...");
                    }
                },
                async (dc, args, next) =>
                {
                    string whatYouSaid = (string)args["Value"];
                    if (whatYouSaid.ToLower() == "done")
                    {
                        await dc.Replace("initial");
                    }
                    else
                    {
                        await dc.Prompt("textPrompt", $"You said {whatYouSaid}");
                        await dc.Replace("echo");
                    }
                }
                });

                dialogs.Add("intent", new WaterfallStep[]
                {
                async (dc, args, next) =>
                {

                    var prompt = new ChoicePromptOptions{
                        Choices = new List<Choice>()
                        {
                            new Choice{ Value = "Latest Checkstub" },
                            new Choice{ Value = "Retirement Plans" },
                            new Choice{ Value = "PTO Balances" },
                            new Choice{ Value = "I'm Done Here" }
                        }
                    };

                    var convo = ConversationState<Dictionary<string, object>>.Get(dc.Context);
                    await dc.Prompt("choicePrompt", $"Go ahead {convo["Name"]}, what do you want to view?", prompt);
                },
                async (dc, args, next) =>
                {
                    var foundChoice = (FoundChoice)args["Value"];

                    await dc.Prompt("textPrompt", $"You selected: {foundChoice.Value}");

                    if (foundChoice.Value == "I'm Done Here")
                    {
                        await dc.Replace("initial");
                    }
                    else
                    {
                        await dc.Replace("intent");
                    }
                }
                });

                dialogs.Add("textPrompt", new TextPrompt());
                dialogs.Add("choicePrompt", new ChoicePrompt(Culture.English));
            }
        }

        public async Task OnTurn(ITurnContext context)
        {
                var state = context.GetConversationState<Dictionary<string, object>>();
            var dialogCtx = dialogs.CreateContext(context, state);

            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Bot says hello");

                            var quickReplies = MessageFactory.SuggestedActions(new CardAction[] {
                                new CardAction(title: "Yeah!", type: ActionTypes.ImBack, value: "Yes"),
                                new CardAction( title: "Nope!", type: ActionTypes.ImBack, value: "No")
                            }, text: "Do you want to see what I can do?");

                            Thread.Sleep(1500);
                            await context.SendActivity(quickReplies);
                        }
                    }
                    break;

                case ActivityTypes.Message:
                    await dialogCtx.Continue();
                    switch (context.Activity.Text)
                    {
                        case "Yes":
                            await dialogCtx.Begin("initial");
                            break;

                        case "No":
                            await dialogCtx.Context.SendActivity("Ok, your loss mate!");
                            break;

                        default:
                            if (! dialogCtx.Context.Responded)
                            {
                                var convo = ConversationState<Dictionary<string, object>>.Get(dialogCtx.Context);
                                await dialogCtx.Context.SendActivity($"I'm ignoring you {convo["Name"]}...");
                            }
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
