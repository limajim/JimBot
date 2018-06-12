using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace AIFunctionality
{
    public class ProfileControl : DialogContainer
    {
        public ProfileControl()
            : base("fillProfile")
        {
            Dialogs.Add("fillProfile",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("textPrompt", "What's your name?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["name"] = args["Value"];
                        await dc.Prompt("textPrompt", "What's your phone number?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["phone"] = args["Value"];
                        await dc.End(dc.ActiveDialog.State);
                    }
                }
            );
            Dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }
    }
}
