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
    public class AddProductCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public AddProductCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message)
        {
            return (
                (message.Text == CommandConstants.ADD_PRODUCT_COMMAND && !_stateMachine.HasState(message.Chat.Id))
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.AddingProduct
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.AddingIngridientsForProduct
            );
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
                case UserState.AddingProduct:
                    {
                        await SecondStage(message);
                        break;
                    }
                case UserState.AddingIngridientsForProduct:
                    {
                        await ThirdStage(message);
                        break;
                    }
            }
        }

        private async Task FirstStage(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user?.Role == BotConstants.ADMIN_ROLE)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: GetProductTemplate());
                _stateMachine.UpdateState(message.Chat.Id, UserState.AddingProduct);
                _logger.Information("{0} has entered /add_product command successfully", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /add_product command unsuccessfully", message?.From?.Username);
            }
        }

        private async Task SecondStage(Message message)
        {
            string[] lines = message.Text!.Split('\n');
            try
            {
                var product = new Product
                {
                    Category = lines[0],
                    Name = lines[1],
                    Description = lines[2],
                    Price = decimal.Parse(lines[3]),
                    Weight = int.Parse(lines[4])
                };
                await _dBService.Create(product);

                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Товар успешно добавлен.");
                _stateMachine.UpdateProduct(message.Chat.Id, product);
                _logger.Information("{0} added product successfully", message?.From?.Username);

                await _botClient.SendMessage(chatId: message!.Chat.Id, text: GetIngridientsTemplate());
                _stateMachine.UpdateState(message.Chat.Id, UserState.AddingIngridientsForProduct);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Не получилось добавить товар. Проверьте соответствие шаблону и попробуйте снова.");
                _logger.Error("An error ocurred while {0} adding product: {1}", message?.From?.Username, ex.Message);
            }
        }

        private async Task ThirdStage(Message message)
        {
            string[] lines = message.Text!.Split('\n');
            try
            {
                var product = _stateMachine.GetProduct(message.Chat.Id);
                foreach (var line in lines)
                {
                    string[] ingredientAmount = line.Split(':');
                    var ingredients = await _dBService.GetAll<Ingredient>();
                    var existIngredient = ingredients.FirstOrDefault(x => x.Name == ingredientAmount[0]);
                    if (existIngredient != null)
                    {
                        await _dBService.Create(new ProductIngredient
                        {
                            ProductId = product!.Id,
                            IngredientId = existIngredient.Id,
                            Amount = int.Parse(ingredientAmount[1])
                        });
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId: message.Chat.Id, text: $"Ингредиент под названием \"{ingredientAmount[0]}\" не найден на складе.");
                        throw new Exception("Ingredient not found");
                    }
                }
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Ингредиенты успешно добавлены.");
                _logger.Information("{0} added ingredients for product successfully", message?.From?.Username);
                _stateMachine.DeleteState(message!.Chat.Id);
                _stateMachine.DeleteProduct(message!.Chat.Id);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Не получилось добавить ингредиенты. Проверьте соответствие шаблону и попробуйте снова.");
                _logger.Error("An error ocurred while {0} adding ingredients for product: {1}", message?.From?.Username, ex.Message);
            }
        }

        private static string GetProductTemplate()
        {
            string description = "Пицца Моцарелла — это классическое итальянское блюдо, которое сочетает в себе нежность сыра моцарелла и насыщенный вкус томатного соуса. Тесто для этой пиццы тонкое и хрустящее, идеально дополняющее начинку.";
            string text =
                $"Введите информацию о продукте по следующему шаблону:\n\n" +
                $"Пицца\n" +
                $"Моцарелла\n" +
                $"{description}\n" +
                $"450,00\n" +
                $"500\n\n" +
                $"Категория должна быть одной из следующих: Пицца, Суши, Напитки, Десерты. " +
                $"Снизу вверх идет категория, название, описание, цена и вес.";
            return text;
        }

        private static string GetIngridientsTemplate()
        {
            string text =
                $"Теперь добавьте ингридиенты по следующему шаблону:\n\n" +
                $"Тесто:500\n" +
                $"Томатная паста:50\n" +
                $"Курица:200\n" +
                $"Сыр:100\n" +
                $"Упаковка для пиццы:1\n\n" +
                $"Сначала идет название ингридиента (должно в точности соответствовать названию на складе), а потом количество в граммах или штуках.";
            return text;
        }
    }
}