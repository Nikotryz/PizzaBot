using PizzaBot.Models;

namespace PizzaBot.Bot.BotService
{
    public class UserStateMachine
    {
        private readonly Dictionary<long, UserState> _userStates = new();
        private readonly Dictionary<long, Product> _userProducts = new();
        private readonly Dictionary<long, Order> _userOrders = new();

        public bool HasState(long userId) => _userStates.ContainsKey(userId);
        public bool HasProduct(long userId) => _userProducts.ContainsKey(userId);
        public bool HasOrder(long userId) => _userOrders.ContainsKey(userId);

        public UserState? GetState(long userId) => HasState(userId) ? _userStates[userId] : null;
        public Product? GetProduct(long userId) => HasProduct(userId) ? _userProducts[userId] : null;
        public Order? GetOrder(long userId) => HasOrder(userId) ? _userOrders[userId] : null;

        public void UpdateState(long userId, UserState state) => _userStates[userId] = state;
        public void UpdateProduct(long userId, Product product) => _userProducts[userId] = product;
        public void UpdateOrder(long userId, Order order) => _userOrders[userId] = order;

        public void DeleteState(long userId) => _userStates.Remove(userId);
        public void DeleteProduct(long userId) => _userProducts.Remove(userId);
        public void DeleteOrder(long userId) => _userOrders.Remove(userId);
    }

    public enum UserState
    {
        Ordering,
        AddingIngridientsToWarehouse,
        AddingProduct,
        AddingIngridientsForProduct
    }
}
