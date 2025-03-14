using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Models;
using PizzaBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot;
using Serilog;

namespace PizzaBot.Bot.BotCommands.ClientCommands
{
    public class CheckClientOrderHistoryCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CheckClientOrderHistoryCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message) => message.Text == CommandConstants.CHECK_CLIENT_ORDER_HISTORY_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var clientId = message.Chat.Id;
            var orders = await _dBService.GetAll<Order>();
            var clientOrders = orders.Where(x => x.ClientId == clientId && x.Status == BotConstants.ORDER_COMPLETED);
            if (orders.Count != 0)
            {
                await _botClient.SendMessage(
                    chatId: clientId,
                    text: "История ваших заказов:");
                foreach (var order in clientOrders)
                {
                    await _botClient.SendMessage(
                        chatId: clientId,
                        text: await PrintOrder(order),
                        replyMarkup: Keyboards.GetMainMenu()
                    );
                }
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: clientId,
                    text: "История ваших заказов пуста.");
            }
        }

        private async Task<string> PrintOrder(Order order)
        {
            var products = await _dBService.GetAll<Product>();
            string text = $"Заказ №{order.Id}\n";
            foreach (var product in order.OrderProducts)
            {
                text += $"{products.FirstOrDefault(x => x.Id == product.ProductId)?.Name} ({products.FirstOrDefault(x => x.Id == product.ProductId)?.Price}) - {product.Amount} шт.\n";
            }
            text += $"\nТоваров на сумму: {order.ProductsCost}\nАдрес доставки: {order.Address}\nСтоимость доставки: {order.DeliveryCost}\n\nИтого: {order.ProductsCost + order.DeliveryCost}";
            return text;
        }
    }
}
