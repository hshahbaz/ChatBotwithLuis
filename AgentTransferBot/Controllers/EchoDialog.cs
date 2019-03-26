using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        private readonly IUserToAgent _userToAgent;

        public EchoDialog(IUserToAgent userToAgent)
        {
            _userToAgent = userToAgent;
        }
        public void Start(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        public Task StartAsync(IDialogContext context)
        {
            throw new NotImplementedException();
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Text.StartsWith("a"))
            {
                var agent = await _userToAgent.IntitiateConversationWithAgentAsync(message as Activity, default(CancellationToken));
                if (agent == null)
                    await context.PostAsync("All our customer care representatives are busy at the moment. Please try after some time.");
            }
            else
            {
                await context.PostAsync(message.Text);
            }
            context.Done(true);
        }
    }
}
