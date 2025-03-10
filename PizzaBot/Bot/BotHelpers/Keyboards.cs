using PizzaBot.Bot.BotCommands.CommandService;
using Telegram.Bot.Types.ReplyMarkups;

namespace PizzaBot.Bot.BotHelpers
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup GetMainMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.CHECK_CATALOG_COMMAND, CommandConstants.PLACE_ORDER_COMMAND},
                new KeyboardButton[] {"История заказов"}
            })
            { 
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetCurierMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {"Просмотреть активный заказ"},
                new KeyboardButton[] {"История заказов"}
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetAdminMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.ADD_PRODUCT_COMMAND, CommandConstants.ADD_INGRIDIENTS_TO_WAREHOUSE_COMMAND},
                new KeyboardButton[] {"Добавить зону доставки", CommandConstants.CHECK_INGREDIENTS_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}