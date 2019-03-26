using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AgentTransferBot
{
    [Serializable]
    [LuisModel("b88a797a-4876-4bc5-a429-6a7ea0f6ca9f", "31508b3f03374b689d159876e01f3a10a")]
    public class TransferLuisDialog : LuisDialog<object>
    {
        private readonly IUserToAgent _userToAgent;

        public TransferLuisDialog(IUserToAgent userToAgent)
        {

            _userToAgent = userToAgent;
        }
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            await context.PostAsync("Sorry, I wasn't trained for this.");
            await context.PostAsync("Please rephrase your question or type \"Customer service\" to talk to agent Larry");
            context.Done<object>(null);
        }

        [LuisIntent("Greeting")]
        public async Task Greetings(IDialogContext context, LuisResult luisResult)
        {
            if(luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            } else
            {
                await context.PostAsync("Hello");
            }
            context.Done<object>(null);
        }
        [LuisIntent("LendingStates")]
        public async Task LendingStates(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
              await context.PostAsync("AZ, CA, CO, FL, GA, HI, NC, NV, OR, SC, TN, TX, UT, VA & WA");
            }
            context.Done<object>(null);
        }
        [LuisIntent("Broker")]
        public async Task Broker(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Yes, click here to fill out the broker agreement https://www.origin8now.com/app/register");
            }
            context.Done<object>(null);
        }

        [LuisIntent("CloseCase")]
        public async Task CloseCase(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("8 to 10 days, it all depends on how long it takes to get access to the property.");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Location")]
        public async Task Location(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Our list of branches are here https://www.civicfs.com/branches/) ");
            }
            context.Done<object>(null);
        }


        [LuisIntent("NextStep")]
        public async Task NextStep(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Click here to submit your loan https://www.origin8now.com/app/loan/ or to check pricing click here https://www.origin8now.com/app/quick-quote");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Rates")]
        public async Task Rates(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Our rates start at 7.99%, it all depends on your level of investment experience ");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Fees")]
        public async Task Fees(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("We typically charge 2 pts, or 2% of the loan amount plus our $1,195 processing fees.");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Brokers")]
        public async Task Brokers(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Yes, click here to fill out the broker agreement https://www.origin8now.com/app/register");
            }
            context.Done<object>(null);
        }

        [LuisIntent("LTV")]
        public async Task LTV(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("Our max loan to value is 80%, but that depends on the location of the property." +
                                        " Where is the property located?");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Rehab")]
        public async Task Rehab(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("We typically provide up to 100% of the rehab financing.  This will depend on the scope of rehab work and your overall strategy.");
            }
            context.Done<object>(null);
        }
        [LuisIntent("OfficeTimings")]
        public async Task OfficeTimings(IDialogContext context, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                await context.PostAsync("We are open from Monday through Friday from 8:00am to 6:00pm.");
            }
            context.Done<object>(null);
        }
        [LuisIntent("AgentTransfer")]
        public async Task Location(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                var activity = await message;
                var agent = await _userToAgent.IntitiateConversationWithAgentAsync(activity as Activity, default(CancellationToken));
                if (agent == null)
                    await context.PostAsync("All our customer care representatives are busy at the moment. Please try after some time.");
            }
            context.Done<object>(null);
        }
        [LuisIntent("Menu")]
        public async Task ShowMenu(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                Activity reply = ((Activity)context.Activity).CreateReply();
                string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\MyCard.json"));
                // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
                // put the adaptive card as an attachment to the reply message
                reply.Attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                });
                await context.PostAsync(reply);
            }
            context.Done<object>(null);
        }
    }
}
