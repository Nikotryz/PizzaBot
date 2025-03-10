namespace PizzaBot.Bot.BotCommands.CommandService
{
    public static class CommandConstants
    {
        public const string START_COMMAND = "/start";
        public const string HELP_COMMAND = "/help";
        public const string ADMIN_COMMAND = "/admin";

        public const string CHECK_CATALOG_COMMAND = "Каталог товаров";
        public const string PLACE_ORDER_COMMAND = "Оформить заказ";

        public const string ADD_PRODUCT_COMMAND = "Добавить товар в каталог";
        public const string ADD_INGRIDIENTS_TO_WAREHOUSE_COMMAND = "Добавить провизию на склад";
        public const string CHECK_INGREDIENTS_COMMAND = "Ингредиенты на складе";

        public const string UNPERMISSIONED_COMMAND_WARNING = "У вас нет прав на выполнение этой команды";
    }
}
