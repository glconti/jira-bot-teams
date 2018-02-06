using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace JiraBot.MessageHandlers
{
    public interface IMessageHandler
    {
        Task AnswerAsync(IDialogContext context, Activity activity, string message);
    }
}