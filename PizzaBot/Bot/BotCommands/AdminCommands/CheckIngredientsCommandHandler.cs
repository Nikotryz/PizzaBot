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
    public class CheckIngredientsCommandHandler : ICommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateMachine _stateMachine;
        private readonly DBService _dBService;
        private readonly ILogger _logger;

        public CheckIngredientsCommandHandler(ITelegramBotClient botClient, UserStateMachine stateMachine, DBService dbService, ILogger logger)
        {
            _botClient = botClient;
            _stateMachine = stateMachine;
            _dBService = dbService;
            _logger = logger;
        }

        public bool CanHandle(Message message) => message.Text == CommandConstants.CHECK_INGREDIENTS_COMMAND && !_stateMachine.HasState(message.Chat.Id);

        public async Task HandleAsync(Message message)
        {
            var user = await _dBService.GetById<Models.User>(message.Chat.Id);
            if (user?.Role == BotConstants.ADMIN_ROLE)
            {
                var ingredients = await _dBService.GetAll<Ingredient>();
                if (ingredients.Count != 0)
                {
                    string text = "";
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        text += $"{i + 1}: {ingredients[i].Name} - {ingredients[i].Amount}\n";
                    }
                    await _botClient.SendMessage(chatId: message.Chat.Id, text: text);
                }
                else
                {
                    await _botClient.SendMessage(chatId: message.Chat.Id, text: "На складе отсутствуют ингредиенты.");
                }
                _logger.Information("{0} has entered /check_ingredients command successfully", message?.From?.Username);
            }
            else
            {
                await _botClient.SendMessage(chatId: message.Chat.Id, text: CommandConstants.UNPERMISSIONED_COMMAND_WARNING);
                _logger.Warning("{0} has entered /check_ingredients command unsuccessfully", message?.From?.Username);
            }
        }
    }
}
