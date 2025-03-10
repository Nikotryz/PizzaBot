using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace PizzaBot.Bot.BotService
{
    public class BotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UpdateHandler _updateHandler;

        public BotService(ITelegramBotClient botClient, UpdateHandler updateHandler)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var cts = new CancellationTokenSource();
            _botClient.StartReceiving(
                _updateHandler.HandleUpdateAsync,
                _updateHandler.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            await Task.Delay(-1, cancellationToken);
        }
    }
}
