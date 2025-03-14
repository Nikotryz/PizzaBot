using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot;
using Serilog;

namespace PizzaBot.Bot.BotCommands.AdminCommands
{
    public class ChangeCostPerKilometerCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public ChangeCostPerKilometerCommandHandler(ITelegramBotClient botClient, DBService dBService, UserStateMachine stateMachine, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dBService;
            _logger = logger;
        }
        public bool CanHandle(Message message)
        {
            return
                (message.Text == CommandConstants.CHANGE_DELIVERY_COST_PER_KILOMETER && !_stateMachine.HasState(message.Chat.Id))
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.EnteringDeliveryCostPerKilometer;
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
                case UserState.EnteringDeliveryCostPerKilometer:
                    {
                        await SecondStage(message);
                        break;
                    }
            }
        }

        private async Task FirstStage(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user?.Role == BotConstants.ADMIN_ROLE)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Введите новую стоимость километра доставки:");
                _stateMachine.UpdateState(message.Chat.Id, UserState.EnteringDeliveryCostPerKilometer);
                _logger.Information("{0} has entered /change_delivery_cost_per_kilometer command successfully", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /change_delivery_cost_per_kilometer command unsuccessfully", message?.From?.Username);
            }
        }

        private async Task SecondStage(Message message)
        {
            try
            {
                var newCost = decimal.Parse(message.Text!);
                DeliveryCalculator.ChangeCostPerKilometer(newCost);
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Стоимость километра доставки изменена.");
                _stateMachine.DeleteState(message!.Chat.Id);
                _logger.Information("{0} changed delivery cost per kilometer to {1}", message?.From?.Username, newCost);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Не получилось поменять стоимость километра доставки. Проверьте правильность написания стоимости и попробуйте снова.");
                _logger.Error("An error ocurred: {0}", ex.Message);
            }
        }
    }
}
