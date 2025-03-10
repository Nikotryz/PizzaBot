using Telegram.Bot;
using Telegram.Bot.Types;
using Serilog;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Services;
using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotService;

namespace PizzaBot.Bot.Commands.ClientCommands
{
    public class StartCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public StartCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message) => message.Text == CommandConstants.START_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user == null)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Рады видеть Вас, новый пользователь! Выберите действие:", replyMarkup: Keyboards.GetMainMenu());
                await _dBService.Create(new Models.User
                {
                    Id = message.Chat.Id,
                    Role = "Клиент"
                });
                _logger.Information("New user added to data base: {0}", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Рады снова видеть вас! Выберите действие:", replyMarkup: Keyboards.GetMainMenu());
                _logger.Information("{0} has entered the /start command", message?.From?.Username);
            }
        }
    }
}
