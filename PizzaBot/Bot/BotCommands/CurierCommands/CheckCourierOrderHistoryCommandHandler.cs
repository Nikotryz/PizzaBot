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
    public class CheckCourierOrderHistoryCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CheckCourierOrderHistoryCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message) => message.Text == CommandConstants.CHECK_COURIER_ORDER_HISTORY_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user?.Role == BotConstants.EMPLOYEE_ROLE)
            {
                var courierId = message.Chat.Id;
                var orders = await _dBService.GetAll<Order>();
                var completedOrders = orders.Where(x => x.CourierId == courierId && x.Status == BotConstants.ORDER_COMPLETED);
                await _botClient.SendMessage(
                    chatId: courierId,
                    text: "История ваших выполненных заказов:"
                );
                foreach (var order in completedOrders)
                {
                    await _botClient.SendMessage(
                        chatId: courierId,
                        text: await PrintOrder(order),
                        replyMarkup: Keyboards.GetCurierMenu()
                    );
                }
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /check_courier_order_history command unsuccessfully", message?.From?.Username);
            }
        }

        private async Task<string> PrintOrder(Order order)
        {
            var products = await _dBService.GetAll<Product>();
            string text = "";
            foreach (var product in order.OrderProducts)
            {
                text += $"{order.Id}\n\n";
                text += $"{products.FirstOrDefault(x => x.Id == product.ProductId)?.Name} - {product.Amount} шт.\n";
            }
            text += $"\nАдрес доставки: {order.Address}\nСтоимость доставки: {order.DeliveryCost}\nДата выполнения: {order.PerformedAt}";
            return text;
        }
    }
}
