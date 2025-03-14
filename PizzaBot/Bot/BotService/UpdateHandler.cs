using Telegram.Bot;
using Telegram.Bot.Types;
using Serilog;
using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotCommands.ClientCommands;

namespace PizzaBot.Bot.BotService
{
    public class UpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly CommandHandlerFactory _commandFactory;
        private readonly PlaceOrderCommandHandler _placeOrderCommandHandler;
        private readonly ILogger _logger;

        public UpdateHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, CommandHandlerFactory commandFactory, PlaceOrderCommandHandler placeOrderCommandHandler, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _commandFactory = commandFactory;
            _placeOrderCommandHandler = placeOrderCommandHandler;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var handler = _commandFactory.GetHandler(update.Message!);
                if (handler != null)
                {
                    await handler.HandleAsync(update.Message!);
                }
                else
                {
                    await _botClient.SendMessage(chatId: update.Message!.Chat.Id, text: "Неизвестная команда");
                    _logger.Information("{0} entered unknown command: {1}", update?.Message?.From?.Username, update?.Message?.Text);
                }
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && _stateMachine.GetState(update.CallbackQuery!.Message!.Chat.Id) == UserState.ConfirmingOrder)
            {
                await _placeOrderCommandHandler.FourthStage(update.CallbackQuery!);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error("An error occurred: {0}", exception.Message);
        }
    }
}
