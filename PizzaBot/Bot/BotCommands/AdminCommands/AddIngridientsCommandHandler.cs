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
    public class AddIngridientsCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public AddIngridientsCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }
        public bool CanHandle(Message message)
        {
            return 
                message.Text == CommandConstants.ADD_INGRIDIENTS_TO_WAREHOUSE_COMMAND && !_stateMachine.HasState(message.Chat.Id)
                ||
                _stateMachine.GetState(message.Chat.Id) == UserState.AddingIngridientsToWarehouse
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
                case UserState.AddingIngridientsToWarehouse:
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
                await _botClient.SendMessage(chatId: message.Chat.Id, text: GetIngridientsTemplate());
                _stateMachine.UpdateState(message.Chat.Id, UserState.AddingIngridientsToWarehouse);
                _logger.Information("{0} has entered /add_ingredients command successfully", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /add_ingredients command unsuccessfully", message?.From?.Username);
            }
        }

        private async Task SecondStage(Message message)
        {
            string[] lines = message.Text!.Split('\n');
            try
            {
                foreach (var line in lines)
                {
                    string[] ingredientAmount = line.Split(':');
                    var ingredients = await _dBService.GetAll<Ingredient>();
                    var existIngredient = ingredients.FirstOrDefault(x => x.Name == ingredientAmount[0]);
                    if (existIngredient != null)
                    {
                        existIngredient.Amount += int.Parse(ingredientAmount[1]);
                        await _dBService.Update(existIngredient);
                    }
                    else
                    {
                        var ingredient = new Ingredient
                        {
                            Name = ingredientAmount[0],
                            Amount = int.Parse(ingredientAmount[1])
                        };
                        await _dBService.Create(ingredient);
                    }
                }
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Ингредиенты успешно добавлены.");
                _logger.Information("{0} added ingredients successfully", message?.From?.Username);
                _stateMachine.DeleteState(message!.Chat.Id);
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: "Не получилось добавить ингредиенты. Проверьте соответствие шаблону и попробуйте снова.");
                _logger.Error("An error ocurred while {0} adding ingredients: {1}", message?.From?.Username, ex.Message);
            }
        }

        private static string GetIngridientsTemplate()
        {
            string text =
                $"Добавьте ингредиенты на склад по следующему шаблону:\n\n" +
                $"Тесто:5000\n" +
                $"Томатная паста:500\n" +
                $"Курица:2000\n" +
                $"Сыр:1000\n" +
                $"Упаковка для пиццы:10\n\n" +
                $"Сначала идет название ингредиента, а потом количество в граммах или штуках.";
            return text;
        }
    }
}
