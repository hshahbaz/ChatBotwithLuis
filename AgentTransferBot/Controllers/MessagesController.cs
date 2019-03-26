using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Threading;
using static AgentTransferBot.Utilities;
using System.Web;
using System.IO;
using AdaptiveCards;
using AgentTransferBot.Dialogs;

namespace AgentTransferBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                
                    await SendAsync(activity, (scope) => new RootDialog(scope.Resolve<IUserToAgent>()));

                
                //if (activity.Value != null)
                //{
                //    //using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                //    //{
                //    //    var client = scope.Resolve<IConnectorClient>();
                //    //    var reply1 = activity.CreateReply();
                //    //    reply1.Text = "going to root";
                //    //    await client.Conversations.ReplyToActivityAsync(reply1);
                //    //}
                //    //await Conversation.SendAsync(activity, () => new RootDialog());
                //}
                //else
                //{
                //    await SendAsync(activity, (scope) => new TransferLuisDialog(scope.Resolve<IUserToAgent>()));
                //}

            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = message;
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (update.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in update.MembersAdded)
                        {
                            //var reply1 = message.CreateReply();
                            //reply1.Text = "new member" +  newMember.Name.ToLower();
                            //await client.Conversations.ReplyToActivityAsync(reply1);
                            // the bot is always added as a user of the conversation, since we don't
                            // want to display the adaptive card twice ignore the conversation update triggered by the bot
                            if (newMember.Name.ToLower() != "you")
                            {
                                try
                                {
                                    // read the json in from our file
                                    string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\MyCard.json"));
                                    // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                                    AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
                                    // put the adaptive card as an attachment to the reply message
                                    reply.Attachments.Add(new Attachment
                                    {
                                        ContentType = AdaptiveCard.ContentType,
                                        Content = card
                                    });
                                }
                                catch (Exception e)
                                {
                                    // if an error occured add the error text as the message
                                    reply.Text = e.Message;
                                }
                                await client.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            else if (message.Type == ActivityTypes.Event)
            {
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var cancellationToken = default(CancellationToken);
                    var agentService = scope.Resolve<IAgentService>();
                    switch (message.AsEventActivity().Name)
                    {
                        case "connect":
                            await agentService.RegisterAgentAsync(message, cancellationToken);
                            break;
                        case "disconnect":
                            await agentService.UnregisterAgentAsync(message, cancellationToken);
                            break;
                        case "stopConversation":
                            await StopConversation(agentService, message, cancellationToken);
                            await agentService.RegisterAgentAsync(message, cancellationToken);
                            break;
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        private async Task StopConversation(IAgentService agentService, Activity agentActivity, CancellationToken cancellationToken)
        {
            var user = await agentService.GetUserInConversationAsync(agentActivity, cancellationToken);
            var agentReply = agentActivity.CreateReply();
            if (user == null)
            {
                agentReply.Text = "Hey! You were not talking to anyone.";
                await SendToConversationAsync(agentReply);
                return;
            }

            var userActivity = user.ConversationReference.GetPostToBotMessage();
            await agentService.StopAgentUserConversationAsync(
                userActivity,
                agentActivity,
                cancellationToken);

            var userReply = userActivity.CreateReply();
            userReply.Text = "You have been disconnected from our representative.";
            await SendToConversationAsync(userReply);
            userReply.Text = "But we can still talk :)";
            await SendToConversationAsync(userReply);

            agentReply.Text = "You have stopped the conversation.";
            await SendToConversationAsync(agentReply);
        }

        private async Task SendAsync(IMessageActivity toBot, Func<ILifetimeScope, IDialog<object>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, toBot))
            {
                DialogModule_MakeRoot.Register(scope, () => MakeRoot(scope));
                var task = scope.Resolve<IPostToBot>();
                await task.PostAsync(toBot, token);
            }
        }
    }
}