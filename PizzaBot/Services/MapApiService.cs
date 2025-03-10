using Newtonsoft.Json.Linq;
using Serilog;

namespace PizzaBot.Services
{
    public class MapApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        private const string BASE_URL = "https://geocode-maps.yandex.ru/1.x/";
        private const string API_KEY = "69f42807-c1ca-475a-b770-387119825cb1";

        public MapApiService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetArea(string address)
        {
            try
            {
                var coordinates = await GetCoordinates(address);
                var response = await _httpClient.GetAsync($"{BASE_URL}?apikey={API_KEY}&geocode={Uri.EscapeDataString(coordinates)}&lang=ru_RU&kind=district&results=1&format=json");

                if (response.IsSuccessStatusCode)
                {
                    string stringResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(stringResponse);

                    JArray? features = (JArray?)jsonResponse?["response"]?["GeoObjectCollection"]?["featureMember"];

                    JArray? components = (JArray?)features?[0]?["GeoObject"]?["metaDataProperty"]?["GeocoderMetaData"]?["Address"]?["Components"];

                    foreach (var item in components)
                    {
                        if (item?["kind"]?.Value<string>() == "district")
                        {
                            return item?["name"]?.Value<string>();
                        }
                    }
                }
                else
                {
                    _logger.Error("Error when executing a request: {0}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetArea(): {0}", ex.Message);
            }

            return null;
        }

        private async Task<string?> GetCoordinates(string address)
        {
            string? coordinates = null;

            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}?apikey={API_KEY}&geocode={Uri.EscapeDataString(address)}&lang=ru_RU&results=1&format=json");

                if (response.IsSuccessStatusCode)
                {
                    string stringResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(stringResponse);

                    JArray? features = (JArray?)jsonResponse?["response"]?["GeoObjectCollection"]?["featureMember"];

                    coordinates = features?[0]?["GeoObject"]?["Point"]?["pos"]?.Value<string>();
                }
                else
                {
                    _logger.Error("Error when executing a request: {0}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetArea(): {0}", ex.Message);
            }

            return $"{coordinates?.Split(' ')[0]}, {coordinates?.Split(' ')[1]}";
        }
    }
}