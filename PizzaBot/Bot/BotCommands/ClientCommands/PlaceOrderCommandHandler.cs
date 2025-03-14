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
    public class PlaceOrderCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly DeliveryCalculator _deliveryCalc;
        private readonly ILogger _logger;

        public PlaceOrderCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, DeliveryCalculator deliveryCalc, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _deliveryCalc = deliveryCalc;
            _logger = logger;
        }
        public bool CanHandle(Message message)
        {
            return
                message.Text == CommandConstants.PLACE_ORDER_COMMAND && !_stateMachine.HasState(message.Chat.Id)
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.EnteringProducts
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.EnteringAddress
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.ConfirmingOrder
            ;
        }

        public async Task HandleAsync(Message message)
        {
            switch (_stateMachine.GetState(message.Chat.Id))
            {
                case null:
                    {
                        await FirstStage(message);
                        break;
                    }
                case UserState.EnteringProducts:
                    {
                        await SecondStage(message);
                        break;
                    }
                case UserState.EnteringAddress:
                    {
                        await ThirdStage(message);
                        break;
                    }
            }
        }

        private async Task FirstStage(Message message)
        {
            await _botClient.SendMessage(chatId: message.Chat.Id, text: GetOrderTemplate());
            _stateMachine.UpdateState(message.Chat.Id, UserState.EnteringProducts);
            _logger.Information("{0} has entered /place_order command", message?.From?.Username);
        }

        private async Task SecondStage(Message message)
        {
            var userId = message.Chat.Id;
            string[] lines = message.Text!.Split('\n');

            try
            {
                var order = new Order
                {
                    ClientId = userId,
                    ProductsCost = 0,
                    DeliveryCost = 0,
                    Status = BotConstants.ORDER_PROCESSING,
                    CreatedAt = DateTime.Now
                };
                var products = await _dBService.GetAll<Product>();

                foreach (var line in lines)
                {
                    string[] productAmount = line.Split(':');
                    var existProduct = products.FirstOrDefault(x => x.Name == productAmount[0]);
                    if (existProduct != null)
                    {
                        order!.ProductsCost += existProduct.Price * int.Parse(productAmount[1]);
                        order.OrderProducts.Add(new OrderProduct
                        {
                            OrderId = 0,
                            ProductId = existProduct.Id,
                            Amount = int.Parse(productAmount[1])
                        });
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId: userId, text: $"Товара под названием \"{productAmount[0]}\" нет в каталоге.");
                        throw new Exception("User entered not existed product");
                    }
                }
                await _botClient.SendMessage(chatId: userId, text: "Товары заказа успешно добавлены. Теперь введите адрес доставки по шаблону:\n\nТомск, ул. Герцена, д. 18");
                _stateMachine.UpdateOrder(userId, order);
                _stateMachine.UpdateState(userId, UserState.EnteringAddress);
                _logger.Information("{0} placed order", message?.From?.Username);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: userId, text: "Не получилось оформить заказ. Проверьте соответствие шаблону и попробуйте снова.");
                _logger.Error("An error ocurred while {0} placing order: {1}", message?.From?.Username, ex.Message);
            }
        }

        private async Task ThirdStage(Message message)
        {
            try
            {
                var address = message.Text;
                var deliveryCost = await _deliveryCalc.CalculateCost(address!);
                var order = _stateMachine.GetOrder(message.Chat.Id);
                order!.Address = address;
                order.DeliveryCost = deliveryCost;
                await _botClient.SendMessage(chatId: message.Chat.Id, text: await PrintOrder(order), replyMarkup: Keyboards.GetOrderConfirming());
                _stateMachine.UpdateState(message.Chat.Id, UserState.ConfirmingOrder);
                _logger.Information("{0} entered address for order: {1}", message?.From?.Username, address);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Не получилось рассчитать стоимость доставки. Проверьте правильность адреса и попробуйте снова.");
                _logger.Error("An error ocurred while {0} entering address: {1}", message?.From?.Username, ex.Message);
            }
        }

        public async Task FourthStage(CallbackQuery query)
        {
            switch (query.Data)
            {
                case "confirm":
                    {
                        await ConfirmOrder(query);
                        break;
                    }
                case "cancel":
                    {
                        await CancelOrder(query);
                        break;
                    }
            }
        }

        private async Task ConfirmOrder(CallbackQuery query)
        {
            var order = _stateMachine.GetOrder(query.Message!.Chat.Id);
            await _dBService.Create(order!);
            await _botClient.SendMessage(
                chatId: query.Message!.Chat.Id,
                text: "Заказ подтвержден."
            );
            _logger.Information("{0} confirmed order {1}", query.From.Username, order!.Id);
            await SendOrderToAdmins(order!);
            _stateMachine.DeleteState(query.Message.Chat.Id);
            _stateMachine.DeleteOrder(query.Message.Chat.Id);
        }

        private async Task CancelOrder(CallbackQuery query)
        {
            var order = _stateMachine.GetOrder(query.Message!.Chat.Id);
            await _botClient.SendMessage(
                chatId: query.Message!.Chat.Id,
                text: "Заказ отменен."
            );
            _stateMachine.DeleteState(query.Message.Chat.Id);
            _stateMachine.DeleteOrder(query.Message.Chat.Id);
            _logger.Information("{0} canceled order {1}", query.From.Username, order!.Id);
        }

        private static string GetOrderTemplate()
        {
            string text =
                $"\U0001F6D2 Оформите заказ по следующему шаблону:\n\n" +
                $"Маргарита:2\n" +
                $"Четыре сыра:1\n\n" +
                $"\u2757 Сначала идет название товара (должно в точности соответствовать названию в каталоге), а потом количество в штуках.";
            return text;
        }

        private async Task<string> PrintOrder(Order order)
        {
            var products = await _dBService.GetAll<Product>();
            string text = "Заказ выглядит так:\n\n";
            foreach (var product in order.OrderProducts)
            {
                text += $"\u2014 {products.FirstOrDefault(x => x.Id == product.ProductId)?.Name} ({products.FirstOrDefault(x => x.Id == product.ProductId)?.Price}\u20BD): {product.Amount} шт.\n";
            }
            text += $"\n\uD83D\uDCB0 Товаров на сумму: {order.ProductsCost}\u20BD\n\uD83C\uDFE0 Адрес доставки: {order.Address}\n\uD83D\uDE97 Стоимость доставки: {order.DeliveryCost}\u20BD\n\n\uD83E\uDDFE Итого: {order.ProductsCost + order.DeliveryCost}\u20BD";
            return text;
        }

        private async Task SendOrderToAdmins(Order order)
        {
            var users = await _dBService.GetAll<Models.User>();
            var admins = users.Where(x => x.Role == BotConstants.ADMIN_ROLE && x.Id != order.ClientId);
            foreach (var admin in admins)
            {
                await _botClient.SendMessage(
                    chatId: admin.Id, 
                    text: $"Поступил новый заказ! {await PrintOrder(order!)}",
                    replyMarkup: Keyboards.GetAdminOrderConfirming()
                );
                _stateMachine.UpdateState(admin.Id, UserState.ProcessingOrder);
                _stateMachine.UpdateOrder(admin.Id, order);
                _logger.Information("Order {0} sent to admin {1}", order.Id, admin.Id);
            }
        }
    }
}
