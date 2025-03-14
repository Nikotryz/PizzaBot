using PizzaBot.Bot.BotCommands.CommandService;
using PizzaBot.Bot.BotHelpers;
using PizzaBot.Bot.BotService;
using PizzaBot.Models;
using PizzaBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot;
using Serilog;

namespace PizzaBot.Bot.BotCommands.AdminCommands
{
    public class DeleteProductCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public DeleteProductCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message)
        {
            return
                (message.Text == CommandConstants.DELETE_PRODUCT_COMMAND && !_stateMachine.HasState(message.Chat.Id))
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.DeletingProduct
                ;
        }

        public async Task HandleAsync(Message message)
        {
            var userId = message.Chat.Id;
            switch (_stateMachine.GetState(userId))
            {
                case null:
                    {
                        await FirstStage(message);
                        break;
                    }
                case UserState.DeletingProduct:
                    {
                        await SecondStage(message);
                        break;
                    }
            }
        }

        private async Task FirstStage(Message message)
        {
            var userId = message.Chat.Id;

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Введите название товара, который хотите убрать из каталога. Название товара должно в точности соответствовать названию в каталоге.",
                replyMarkup: Keyboards.GetAdminMenu());

            _stateMachine.UpdateState(userId, UserState.DeletingProduct);

            _logger.Information("{0} checked catalog", message?.From?.Username);
        }

        private async Task SecondStage(Message message)
        {
            var userId = message.Chat.Id;
            var productName = message.Text;
            var products = await _dBService.GetAll<Product>();
            var product = products.FirstOrDefault(x => x.Name == productName);
            if (product != null)
            {
                await _dBService.Delete(product);
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Товар успешно удален из каталога.",
                    replyMarkup: Keyboards.GetAdminMenu());
                _stateMachine.DeleteState(userId);
                _logger.Information("{0} deleted {1} from catalog", message?.From?.Username, product.Name);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"Товара с названием \"{productName}\" не существует. Введите название правильно и попробуйте снова.",
                    replyMarkup: Keyboards.GetAdminMenu());
                _logger.Error("{0} entered not existed product while deleting product from catalog: {1}", message?.From?.Username, productName);
            }
        }
    }
}
