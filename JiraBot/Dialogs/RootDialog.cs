using System;
using System.Threading.Tasks;
using JiraBot.MessageHandlers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace JiraBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            await MessageHandlersHub.HandleMessageAsync(context, activity);

            context.Wait(MessageReceivedAsync);
        }
    }
}