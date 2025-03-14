using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotService;
using PizzaBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot;
using Serilog;
using PizzaBot.Models;
using PizzaBot.Bot.BotHelpers;

namespace PizzaBot.Bot.BotCommands.CurierCommands
{
    public class CheckActiveOrderCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CheckActiveOrderCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message) => message.Text == CommandConstants.CHECK_ACTIVE_ORDER_COMMAND && _stateMachine.GetState(message.Chat.Id) == UserState.TakingOrder;

        public async Task HandleAsync(Message message)
        {
            var courierId = message.Chat.Id;
            var orders = await _dBService.GetAll<Order>();
            var activeOrder = orders.FirstOrDefault(x => x.CourierId == courierId && x.Status == BotConstants.ORDER_DELIVERED);
            await _botClient.SendMessage(
                chatId: courierId,
                text: await PrintOrder(activeOrder),
                replyMarkup: Keyboards.GetCurierMenu()
            );
        }

        private async Task<string> PrintOrder(Order order)
        {
            var products = await _dBService.GetAll<Product>();
            string text = "Текущий активный заказ:\n\n";
            foreach (var product in order.OrderProducts)
            {
                text += $"{products.FirstOrDefault(x => x.Id == product.ProductId)?.Name} - {product.Amount} шт.\n";
            }
            text += $"\nАдрес доставки: {order.Address}\nСтоимость доставки: {order.DeliveryCost}";
            return text;
        }
    }
}
