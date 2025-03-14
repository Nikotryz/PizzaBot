using Newtonsoft.Json.Linq;
using Serilog;
using System.Globalization;
using System.Text.Json;

namespace PizzaBot.Services
{
    public class MapApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        private const string TWOGIS_BASE_URL = "https://routing.api.2gis.com/get_dist_matrix";
        private const string TWOGIS_API_KEY = "c31b21ef-febf-48c6-bed8-d02b054839ba";

        private const string YANDEX_BASE_URL = "https://geocode-maps.yandex.ru/1.x/";
        private const string YANDEX_API_KEY = "69f42807-c1ca-475a-b770-387119825cb1";

        public MapApiService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int> GetDistanceAsync(string address)
        {
            try
            {
                var requestUri = $"{TWOGIS_BASE_URL}?key={TWOGIS_API_KEY}&version=2.0&type=shortest";

                var point1 = await GetPointAsync("Томск, ул. Елизаровых, д.43");
                var point2 = await GetPointAsync(address);

                var points = new[]
                {
                    point1,
                    point2
                };

                var sources = new[] { 0 };
                var targets = new[] { 1 };

                var requestBody = new
                {
                    points,
                    sources,
                    targets
                };

                var jsonRequestBody = JsonSerializer.Serialize(requestBody);

                using var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error("Error when executing a request: {0}", response.StatusCode);
                    throw new Exception($"Failed to calculate distance. Status code: {response.StatusCode}");
                }

                var stringResponse = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(stringResponse);

                return jsonResponse["routes"]?[0]?["distance"]?.Value<int>() ?? throw new Exception("Distance value is null");
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetDistanceAsync(): {0}", ex.Message);
                throw;
            }
        }

        private async Task<Point> GetPointAsync(string address)
        {
            try
            {
                var requestUri = $"{YANDEX_BASE_URL}?apikey={YANDEX_API_KEY}&geocode={Uri.EscapeDataString(address)}&lang=ru_RU&results=1&format=json";
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                    var coordinates = jsonResponse["response"]?["GeoObjectCollection"]?["featureMember"]?[0]?["GeoObject"]?["Point"]?["pos"]?.ToString();

                    var splitCoords = coordinates?.Split(' ');
                    var latitude = double.Parse(splitCoords[1], CultureInfo.InvariantCulture);
                    var longitude = double.Parse(splitCoords[0], CultureInfo.InvariantCulture);

                    return new Point(latitude, longitude);
                }
                else
                {
                    _logger.Error("Error when executing a geocoder request: {0}", response.StatusCode);
                    throw new Exception($"Failed to get coordinates for '{address}'. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetPointAsync(): {0}", ex.Message);
                throw;
            }
        }

        private record Point(double lat, double lon);
    }
}