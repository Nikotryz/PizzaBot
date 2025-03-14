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
            var userId = message.Chat.Id;
            var products = await _dBService.GetAll<Product>();

            if (products.Count == 0)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Каталог пуст", replyMarkup: Keyboards.GetMainMenu());
                return;
            }

            await _botClient.SendMessage(chatId: message.Chat.Id, text: "\uD83C\uDF7D Каталог товаров:", replyMarkup: Keyboards.GetMainMenu());

            var pizzaProducts = products.Where(x => x.Category == BotConstants.PIZZA_CATEGORY).ToList();
            var sushiProducts = products.Where(x => x.Category == BotConstants.SUSHI_CATEGORY).ToList();
            var drinkProducts = products.Where(x => x.Category == BotConstants.DRINKS_CATEGORY).ToList();
            var dessertProducts = products.Where(x => x.Category == BotConstants.DESSERTS_CATEGORY).ToList();

            if (pizzaProducts.Count != 0)
            {
                await PrintCategory(pizzaProducts, userId);
            }
            if (sushiProducts.Count != 0)
            {
                await PrintCategory(sushiProducts, userId);
            }
            if (drinkProducts.Count != 0)
            {
                await PrintCategory(drinkProducts, userId);
            }
            if (dessertProducts.Count != 0)
            {
                await PrintCategory(dessertProducts, userId);
            }

            _logger.Information("{0} checked catalog", message?.From?.Username);
        }

        private async Task PrintCategory(List<Product> products, long userId)
        {
            string emoji = products[0].Category switch
            {
                BotConstants.PIZZA_CATEGORY => "\uD83C\uDF55",
                BotConstants.SUSHI_CATEGORY => "\uD83C\uDF63",
                BotConstants.DRINKS_CATEGORY => "\U0001F964",
                BotConstants.DESSERTS_CATEGORY => "\uD83C\uDF70",
                _ => string.Empty
            };
            string text = $"{emoji} {products[0].Category}:\n\n";
            foreach (var product in products)
            {
                text += $"{product.Name} ({product.Weight}г) \u2014 {product.Price}\u20BD\n    {product.Description}\n\n";
            }
            await _botClient.SendMessage(chatId: userId, text: text, replyMarkup: Keyboards.GetMainMenu());
        }
    }
}
