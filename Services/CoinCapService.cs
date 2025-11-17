using CoinUpWorkerService.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoinUpWorkerService.Services
{
    public class CoinCapService : IDataCollectorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CoinCapService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CoinGecko:ApiKey"] ?? throw new InvalidOperationException("API Key manquante dans appsettings.json");
            _httpClient.BaseAddress = new Uri(configuration["CoinGecko:BaseUrl"] ?? throw new InvalidOperationException("Base Url manquante dans appsettings.json"));
            //_httpClient.BaseAddress = new Uri("https://rest.coincap.io/v3/");
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }

        public async Task<List<MarketData>> FetchMarketDataAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>("assets?limit=20");
            var data = new List<MarketData>();

            foreach (var item in response.GetProperty("data").EnumerateArray())
            {
                data.Add(new MarketData
                {
                    AssetId = item.GetProperty("id").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    Symbol = item.GetProperty("symbol").GetString() ?? "",
                    PriceUsd = decimal.Parse(item.GetProperty("priceUsd").GetString() ?? "0"),
                    VolumeUsd24Hr = decimal.Parse(item.GetProperty("volumeUsd24Hr").GetString() ?? "0"),
                    MarketCapUsd = decimal.Parse(item.GetProperty("marketCapUsd").GetString() ?? "0"),
                    ChangePercent24Hr = decimal.Parse(item.GetProperty("changePercent24Hr").GetString() ?? "0")
                });
            }

            return data;
        }
    }
}