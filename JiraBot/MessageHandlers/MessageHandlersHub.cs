using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace JiraBot.MessageHandlers
{
    public static class MessageHandlersHub
    {
        private static readonly List<IMessageHandler> CommandHandlers =
            new List<IMessageHandler>
            {
                new JiraSearcher()
            };

        public static async Task HandleMessageAsync(IDialogContext context, Activity activity)
        {
            var message = activity.RemoveRecipientMention().Trim();

            foreach (var commandHandler in CommandHandlers)
                await commandHandler.AnswerAsync(context, activity, message);
        }
    }
}