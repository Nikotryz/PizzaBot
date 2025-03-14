using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PizzaBot.Services
{
    public class DeliveryCalculator
    {
        private const string DATA_PATH = "D:\\VisualStudio\\Projects\\PizzaBot\\PizzaBot\\WorkingData\\DeliveryData.json";
        private readonly MapApiService _mapApiService;

        public DeliveryCalculator(MapApiService mapApiService) => _mapApiService = mapApiService;

        public async Task<decimal> CalculateCost(string address)
        {
            var distance = await _mapApiService.GetDistanceAsync(address);
            var kilometers = (decimal)(distance * 0.001);
            return GetBaseDeliveryCost() + (kilometers * GetCostPerKilometer());
        }

        private static decimal GetBaseDeliveryCost()
        {
            string data = File.ReadAllText(DATA_PATH);
            JObject jsonData = JObject.Parse(data);
            return jsonData["BaseDeliveryCost"]!.Value<decimal>();
        }

        public static void ChangeBaseDeliveryCost(decimal cost)
        {
            var data = new DeliveryData(cost, GetCostPerKilometer());
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(DATA_PATH, json);
        }

        private static decimal GetCostPerKilometer()
        {
            string data = File.ReadAllText(DATA_PATH);
            JObject jsonData = JObject.Parse(data);
            return jsonData["CostPerKilometer"]!.Value<decimal>();
        }

        public static void ChangeCostPerKilometer(decimal cost)
        {
            var data = new DeliveryData(GetBaseDeliveryCost(), cost);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(DATA_PATH, json);
        }

        private record DeliveryData(decimal BaseDeliveryCost, decimal CostPerKilometer);
    }
}
