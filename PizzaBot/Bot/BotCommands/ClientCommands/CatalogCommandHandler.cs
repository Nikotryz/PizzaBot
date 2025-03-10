using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Models;
using PizzaBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PizzaBot.Bot.Commands.ClientCommands
{
    public class CatalogCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CatalogCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message) => message.Text == CommandConstants.CHECK_CATALOG_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var products = await _dBService.GetAll<Product>();
            if (products.Count != 0)
            {
                string text = "";
                for (int i = 0; i < products.Count; i++)
                {
                    text += $"{i + 1}: {products[i].Name} - {products[i].Price}\n";
                }
                await _botClient.SendMessage(chatId: message.Chat.Id, text: text, replyMarkup: Keyboards.GetMainMenu());
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Каталог пуст", replyMarkup: Keyboards.GetMainMenu());
            }
            _logger.Information("{0} checked catalog", message?.From?.Username);
        }
    }
}
