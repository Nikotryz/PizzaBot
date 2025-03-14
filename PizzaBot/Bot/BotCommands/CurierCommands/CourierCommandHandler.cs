using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PizzaBot.Bot.Commands.CurierCommands
{
    public class CourierCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CourierCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message) => message.Text == CommandConstants.COURIER_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user?.Role == BotConstants.EMPLOYEE_ROLE)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Выберите команду курьер-панели:", replyMarkup: Keyboards.GetCurierMenu());
                _logger.Information("{0} has entered /courier command successfully", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /courier command unsuccessfully", message?.From?.Username);
            }

        }
    }
}
