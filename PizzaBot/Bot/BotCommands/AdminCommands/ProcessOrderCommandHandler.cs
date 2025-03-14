using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Models;
using PizzaBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PizzaBot.Bot.BotCommands.AdminCommands
{
    public class ProcessOrderCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public ProcessOrderCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message) => _stateMachine.GetState(message.Chat.Id) == UserState.ProcessingOrder;

        public async Task HandleAsync(Message message)
        {
            switch (message.Text)
            {
                case CommandConstants.CONFIRM_ORDER_COMMAND:
                    {
                        await ConfirmOrder(message);
                        break;
                    }
                case CommandConstants.CANCEL_ORDER_COMMAND:
                    {
                        await CancelOrder(message);
                        break;
                    }
                case CommandConstants.ORDER_IS_READY_COMMAND:
                    {
                        await OrderIsReady(message);
                        break;
                    }
            }
        }

        private async Task ConfirmOrder(Message message)
        {
            var adminId = message.Chat.Id;
            var order = _stateMachine.GetOrder(adminId);
            order!.Status = BotConstants.ORDER_COOKING;
            await _dBService.Update(order);
            await _botClient.SendMessage(
                chatId: adminId,
                text: $"Заказ принят. Статус заказа изменен на \"{order!.Status}\". Как только заказ будет готов, введите команду \"{CommandConstants.ORDER_IS_READY_COMMAND}\"",
                replyMarkup: Keyboards.GetAdminOrderReady()
            );
            _logger.Information("Admin {0} confirmed the order {1}", adminId, order.Id);
        }

        private async Task CancelOrder(Message message)
        {
            var adminId = message.Chat.Id;
            var order = _stateMachine.GetOrder(adminId);
            await _botClient.SendMessage(
                chatId: order!.ClientId!,
                text: "Ваш заказ был отменен организацией",
                replyMarkup: Keyboards.GetAdminMenu()
            );
            _stateMachine.DeleteOrder(adminId);
            _stateMachine.DeleteState(adminId);
            _logger.Information("Admin {0} canceled the order {1}", adminId, order!.Id);
        }

        private async Task OrderIsReady(Message message)
        {
            var adminId = message.Chat.Id;
            var order = _stateMachine.GetOrder(adminId);
            _logger.Information("The admin {0} has indicated that the order {1} is ready", adminId, order!.Id);
            await SendOrderToCuriers(order!);
            await _botClient.SendMessage(
                chatId: adminId,
                text: $"Заказ был отправлен курьерам.",
                replyMarkup: Keyboards.GetAdminMenu()
            );
            _stateMachine.DeleteOrder(adminId);
            _stateMachine.DeleteState(adminId);
        }

        private async Task SendOrderToCuriers(Order order)
        {
            var users = await _dBService.GetAll<Models.User>();
            var curiers = users.Where(x => x.Role == BotConstants.EMPLOYEE_ROLE && x.Id != order.ClientId);
            foreach (var curier in curiers)
            {
                await _botClient.SendMessage(
                    chatId: curier.Id,
                    text: $"Поступил новый заказ! {await PrintOrder(order!)}",
                    replyMarkup: Keyboards.GetCurierOrderConfirming()
                );
                _stateMachine.UpdateState(curier.Id, UserState.TakingOrder);
                _stateMachine.UpdateOrder(curier.Id, order);
                _logger.Information("Order {0} sent to curier {1}", order.Id, curier.Id);
            }
        }

        private async Task<string> PrintOrder(Order order)
        {
            var products = await _dBService.GetAll<Product>();
            string text = "Заказ выглядит так:\n\n";
            foreach (var product in order.OrderProducts)
            {
                text += $"{products.FirstOrDefault(x => x.Id == product.ProductId)?.Name} ({products.FirstOrDefault(x => x.Id == product.ProductId)?.Price}) - {product.Amount} шт.\n";
            }
            text += $"\nТоваров на сумму: {order.ProductsCost}\nАдрес доставки: {order.Address}\nСтоимость доставки: {order.DeliveryCost}\n\nИтого: {order.ProductsCost + order.DeliveryCost}";
            return text;
        }
    }
}
