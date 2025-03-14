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
                new KeyboardButton[] {CommandConstants.CHECK_CLIENT_ORDER_HISTORY_COMMAND}
            })
            { 
                ResizeKeyboard = true
            };
        }

        public static InlineKeyboardMarkup GetOrderConfirming()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] {
                    InlineKeyboardButton.WithCallbackData("Подтвердить", "confirm"),
                    InlineKeyboardButton.WithCallbackData("Отменить", "cancel"),
                }
            });
        }

        public static ReplyKeyboardMarkup GetCurierMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.CHECK_ACTIVE_ORDER_COMMAND},
                new KeyboardButton[] {CommandConstants.CHECK_COURIER_ORDER_HISTORY_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetCurierOrderConfirming()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.CONFIRM_ORDER_COMMAND},
                new KeyboardButton[] {CommandConstants.CANCEL_ORDER_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetCurierOrderDelivered()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.ORDER_IS_DELIVERED_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetAdminMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.ADD_PRODUCT_COMMAND, CommandConstants.DELETE_PRODUCT_COMMAND},
                new KeyboardButton[] {CommandConstants.CHECK_INGREDIENTS_COMMAND, CommandConstants.ADD_INGRIDIENTS_TO_WAREHOUSE_COMMAND},
                new KeyboardButton[] {CommandConstants.CHANGE_BASE_DELIVERY_COST, CommandConstants.CHANGE_DELIVERY_COST_PER_KILOMETER}
            })
            {
                ResizeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetAdminOrderConfirming()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.CONFIRM_ORDER_COMMAND},
                new KeyboardButton[] {CommandConstants.CANCEL_ORDER_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetAdminOrderReady()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {CommandConstants.ORDER_IS_READY_COMMAND}
            })
            {
                ResizeKeyboard = true
            };
        }

    }
}