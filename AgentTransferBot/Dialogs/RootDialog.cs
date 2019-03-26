using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentTransferBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace AgentTransferBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly IUserToAgent _userToAgent;
        public RootDialog (IUserToAgent userToAgent)
        {
            _userToAgent = userToAgent;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            //await context.PostAsync("root dialog starting...");
            try
            {
                // user's action from Card's actions
                if (activity.Value != null)
                {
                    JToken valueToken = JObject.Parse(activity.Value.ToString());
                    string actionValue = valueToken.SelectToken("action") != null ? valueToken.SelectToken("action").ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(actionValue))
                    {
                        switch (valueToken.SelectToken("action").ToString())
                        {
                            case "quickquote":
                                var survey = new FormDialog<QQForm>(new QQForm(), QQForm.BuildForm, FormOptions.PromptInStart, null);
                                context.Call<QQForm>(survey, AfterSurvey);
                                break;
                            case "civicHowTo":

                                
                                break;
                            case "aboutCivic":
                                var reply1 = activity.CreateReply();
                                //    reply1.Text = "going to root";
                                Attachment attachment = new Attachment();
                                attachment.ContentType = "video/mp4";
                                attachment.ContentUrl = "https://www.youtube.com/watch?v=MlJtRMZQdz0";
                                reply1.Attachments.Add(attachment);
                                await context.PostAsync(reply1);
                                context.Done<object>(null);
                                break;
                                
                            default:
                                await context.PostAsync($"I don't know how to handle the action \"{actionValue}\"");
                                context.Wait(MessageReceivedAsync);
                                break;

                        }
                    }
                    else
                    {
                        await context.PostAsync("It looks like no \"data\" was defined for this. Check your adaptive cards json definition.");
                        context.Wait(MessageReceivedAsync);
                    }
                }       // regular text message from user
                else
                {
                    // LUIS dialog, this is mostly for QnA                    
                   await context.Forward(new TransferLuisDialog(this._userToAgent),AfterLuisDialog, activity, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                // if an error occured with QnAMaker post it out to the user
                await context.PostAsync(e.Message);
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterLuisDialog(IDialogContext context, IAwaitable<object> result)
        {
            //await context.PostAsync("luis done");
            context.Done<object>(null);
        }

        private async Task AfterSurvey(IDialogContext context, IAwaitable<QQForm> result)
        {

            QQForm survey = await result;
            await context.PostAsync("the following quotes rely on the information you provided being true and accurate");
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();

            await context.PostAsync(reply);

            // reset conversations
            context.Done<object>(null);
        }
        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetThumbnailCard(
                    "9.25%",
                    "INTEREST RATE",
                    "$2000 / 2 PTS",
                    new CardImage(url: "https://docs.microsoft.com/en-us/aspnet/aspnet/overview/developing-apps-with-windows-azure/building-real-world-cloud-apps-with-windows-azure/data-storage-options/_static/image5.png"),
                    new CardAction(ActionTypes.OpenUrl, "Email Quote", value: "https://www.origin8now.com")),
                GetThumbnailCard(
                    "9.75%",
                    "INTEREST RATE",
                    "$1500 / 1.75 PTS",
                    new CardImage(url: "https://docs.microsoft.com/en-us/aspnet/aspnet/overview/developing-apps-with-windows-azure/building-real-world-cloud-apps-with-windows-azure/data-storage-options/_static/image5.png"),
                    new CardAction(ActionTypes.OpenUrl, "Email Quote", value: "https://www.origin8now.com")),
                GetThumbnailCard(
                    "10.25%",
                    "INTEREST RATE",
                    "$1000 / 1.5 PTS",
                    new CardImage(url: "https://docs.microsoft.com/en-us/aspnet/aspnet/overview/developing-apps-with-windows-azure/building-real-world-cloud-apps-with-windows-azure/data-storage-options/_static/image5.png"),
                    new CardAction(ActionTypes.OpenUrl, "Email Quote", value: "https://www.origin8now.com"))
            };
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };
            
            return heroCard.ToAttachment();
        }

        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,                
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
    }
}