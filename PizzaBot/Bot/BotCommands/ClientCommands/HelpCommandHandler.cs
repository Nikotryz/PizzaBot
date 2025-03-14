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
            await _botClient.SendMessage(chatId: message.Chat.Id, text: InformationForUsers());
            _logger.Information("{0} has entered the /help command", message?.From?.Username);
        }

        private static string InformationForUsers()
        {
            return
                "Привет, это бот нашей Пиццерии! Он обладает следующими функциями:\n\n" +
                "1) Для клиентов (/start):\n— Просмотр каталога товаров\n— Оформление заказа\n— Просмотр истории заказов\n\n" +
                "2) Для работников (/courier):\n— Просмотр активного заказа\n— Просмотр истории выполненных заказов\n\n" +
                "3) Для администраторов (/admin): \n— Добавление товар в каталог\n— Удаление товара из каталога\n— Добавление провизии на склад\n— Просмотр ингредиентов на складе\n— Изменение стоимости доставки (базовой и за километр)." +
                "\n\nДля выполнения нужных команд нажимайте на них в нижней панели (открыть её можно с помощью квадратика возле сообщения).";
        }
    }
}
