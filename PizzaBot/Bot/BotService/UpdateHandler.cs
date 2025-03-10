using Telegram.Bot;
using Telegram.Bot.Types;
using Serilog;
using PizzaBot.Bot.BotCommands.CommandService;

namespace PizzaBot.Bot.BotService
{
    public class UpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CommandHandlerFactory _commandFactory;
        private readonly ILogger _logger;

        public UpdateHandler(ITelegramBotClient botClient, CommandHandlerFactory commandFactory, ILogger logger)
        {
            _botClient = botClient;
            _commandFactory = commandFactory;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update?.Message?.Text != null)
            {
                var handler = _commandFactory.GetHandler(update.Message);
                if (handler != null)
                {
                    await handler.HandleAsync(update.Message);
                }
                else
                {
                    await _botClient.SendMessage(chatId: update.Message.Chat.Id, text: "Неизвестная команда");
                    _logger.Information("{0} entered: {1}", update?.Message?.From?.Username, update?.Message?.Text);
                }
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error("An error occurred: {0}", exception.Message);
        }
    }
}
