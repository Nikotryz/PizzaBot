using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Models;
using PizzaBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PizzaBot.Bot.BotCommands.CurierCommands;

public class TakeOrderCommandHandler : ICommandHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly UserStateMachine _stateMachine;
    private readonly DBService _dBService;
    private readonly ILogger _logger;

    public TakeOrderCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
    {
        _botClient = botClient;
        _stateMachine = stateMachine;
        _dBService = dbService;
        _logger = logger;
    }
    public bool CanHandle(Message message) => _stateMachine.GetState(message.Chat.Id) == UserState.TakingOrder;

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
            case CommandConstants.ORDER_IS_DELIVERED_COMMAND:
                {
                    await OrderDelivered(message);
                    break;
                }
        }
    }

    private async Task ConfirmOrder(Message message)
    {
        var curierId = message.Chat.Id;
        var order = await _dBService.GetById<Order>(_stateMachine.GetOrder(curierId)!.Id);
        if (order!.CourierId == null)
        {
            order!.Status = BotConstants.ORDER_DELIVERED;
            order!.CourierId = curierId;
            await _dBService.Update(order);
            await _botClient.SendMessage(
                chatId: curierId,
                text: $"Заказ принят. Статус заказа изменен на \"{order!.Status}\". Как только заказ будет доставлен, введите команду \"{CommandConstants.ORDER_IS_DELIVERED_COMMAND}\"",
                replyMarkup: Keyboards.GetCurierOrderDelivered()
            );
            _logger.Information("Curier {0} takes the order {1}", curierId, order.Id);
        }
        else
        {
            await _botClient.SendMessage(
                chatId: curierId,
                text: $"Заказ уже принят другим курьером.",
                replyMarkup: Keyboards.GetCurierOrderDelivered()
            );
            _logger.Information("Curier {0} try to takes the order {1}", curierId, order.Id);
        }
    }

    private async Task CancelOrder(Message message)
    {
        var curierId = message.Chat.Id;
        var order = _stateMachine.GetOrder(curierId);
        await _botClient.SendMessage(
            chatId: curierId,
            text: "Вы отказались от заказа.",
            replyMarkup: Keyboards.GetCurierMenu()
        );
        _stateMachine.DeleteOrder(curierId);
        _stateMachine.DeleteState(curierId);
        _logger.Information("Curier {0} canceled the order {1}", curierId, order!.Id);
    }

    private async Task OrderDelivered(Message message)
    {
        var curierId = message.Chat.Id;
        var order = await _dBService.GetById<Order>(_stateMachine.GetOrder(curierId)!.Id);
        order!.Status = BotConstants.ORDER_COMPLETED;
        order!.PerformedAt = DateTime.Now;
        await _dBService.Update(order);
        await _botClient.SendMessage(
            chatId: curierId,
            text: $"Заказ выполнен.",
            replyMarkup: Keyboards.GetCurierMenu()
        );
        _stateMachine.DeleteOrder(curierId);
        _stateMachine.DeleteState(curierId);
        _logger.Information("Curier {0} delivered the order {1}", curierId, order!.Id);
    }
}