using Telegram.Bot.Types;

namespace PizzaBot.Bot.BotCommands.CommandService
{
    public interface ICommandHandler
    {
        Task HandleAsync(Message message);
        bool CanHandle(Message message);
    }
}
