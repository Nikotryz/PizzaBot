using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotService;
using PizzaBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PizzaBot.Bot.Commands.ClientCommands
{
    public class HelpCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public HelpCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message) => message.Text == CommandConstants.HELP_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            await _botClient.SendMessage(chatId: message.Chat.Id, text: "Информация для пользователей:");
            _logger.Information("{0} has entered the /help command", message?.From?.Username);
        }
    }
}
