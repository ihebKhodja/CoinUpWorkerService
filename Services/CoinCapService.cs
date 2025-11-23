using CoinUpWorkerService.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoinUpWorkerService.Services
{
    public class CoinCapService : IDataCollectorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<CoinCapService> _logger;

        public CoinCapService(HttpClient httpClient, IConfiguration configuration, ILogger<CoinCapService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = configuration["CoinGecko:ApiKey"]
                ?? throw new InvalidOperationException("API Key manquante dans appsettings.json");

            var baseUrl = configuration["CoinGecko:BaseUrl"]
                ?? throw new InvalidOperationException("Base Url manquante dans appsettings.json");

            _httpClient.BaseAddress = new Uri(baseUrl);

            // REQUIRED: CoinGecko blocks requests without User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CoinUpWorker/1.0");

            _httpClient.DefaultRequestHeaders.Add("X-CG-API-KEY", _apiKey);
        }


        public async Task<List<CoinsMarket>> FetchCoinsMarketAsync()
        {

            var items = await _httpClient.GetFromJsonAsync<List<JsonElement>>(
                "coins/markets?vs_currency=usd&per_page=100"
            );

            var result = new List<CoinsMarket>();


            foreach (var item in items)
            {
                result.Add(new CoinsMarket
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Symbol = item.GetProperty("symbol").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    Image = item.GetProperty("image").GetString() ?? "",

                    Current_Price = item.GetProperty("current_price").GetDecimal(),
                    Market_Cap = item.GetProperty("market_cap").GetDecimal(),
                    Market_Cap_Rank = item.GetProperty("market_cap_rank").GetInt32(),
                    Fully_Diluted_Valuation = item.TryGetProperty("fully_diluted_valuation", out var fdv) && fdv.ValueKind != JsonValueKind.Null
                        ? fdv.GetDecimal()
                        : null,

                    Total_Volume = item.GetProperty("total_volume").GetDecimal(),
                    High_24h = item.GetProperty("high_24h").GetDecimal(),
                    Low_24h = item.GetProperty("low_24h").GetDecimal(),
                    Price_Change_24h = item.GetProperty("price_change_24h").GetDecimal(),
                    Price_Change_Percentage_24h = item.GetProperty("price_change_percentage_24h").GetDecimal(),
                    Market_Cap_Change_24h = item.GetProperty("market_cap_change_24h").GetDecimal(),
                    Market_Cap_Change_Percentage_24h = item.GetProperty("market_cap_change_percentage_24h").GetDecimal(),

                    Circulating_Supply = item.GetProperty("circulating_supply").GetDecimal(),
                    Total_Supply = item.TryGetProperty("total_supply", out var ts) && ts.ValueKind != JsonValueKind.Null
                        ? ts.GetDecimal()
                        : null,
                    Max_Supply = item.TryGetProperty("max_supply", out var ms) && ms.ValueKind != JsonValueKind.Null
                        ? ms.GetDecimal()
                        : null,

                    Ath = item.GetProperty("ath").GetDecimal(),
                    Ath_Change_Percentage = item.GetProperty("ath_change_percentage").GetDecimal(),
                    Ath_Date = item.GetProperty("ath_date").GetDateTime(),

                    Atl = item.GetProperty("atl").GetDecimal(),
                    Atl_Change_Percentage = item.GetProperty("atl_change_percentage").GetDecimal(),
                    Atl_Date = item.GetProperty("atl_date").GetDateTime(),

                    Roi = item.TryGetProperty("roi", out var roi)
                        ? roi.ValueKind == JsonValueKind.Null ? null : roi
                        : null,

                    Last_Updated = item.GetProperty("last_updated").GetDateTime()
                });
            }

            return result;
        }

        public async Task<List<CoinsMarketCategory>> FetchMarketCategoriesAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<List<JsonElement>>("coins/categories");

            var result = new List<CoinsMarketCategory>();

            foreach (var item in items)
            {
                result.Add(new CoinsMarketCategory
                {
                    Id = item.GetProperty("id").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "",
                    MarketCap = item.TryGetProperty("market_cap", out var marketCap) && marketCap.ValueKind == JsonValueKind.Number
                        ? marketCap.GetDouble()
                        : 0.0,

                    MarketCapChange24h = item.TryGetProperty("market_cap_change_24h", out var marketCapChange) && marketCapChange.ValueKind == JsonValueKind.Number
                        ? marketCapChange.GetDouble()
                        : 0.0,
                    Content = item.GetProperty("content").GetString() ?? "",

                    Top3CoinsId = item.TryGetProperty("top_3_coins_id", out var top3Ids) && top3Ids.ValueKind == JsonValueKind.Array
                        ? top3Ids.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                        : new List<string>(),

                    Top3Coins = item.TryGetProperty("top_3_coins", out var top3Images) && top3Images.ValueKind == JsonValueKind.Array
                        ? top3Images.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                        : new List<string>(),

                    Volume24h = item.TryGetProperty("volume_24h", out var volume) && volume.ValueKind == JsonValueKind.Number
                        ? volume.GetDouble()
                        : 0.0,  // fallback

                    UpdatedAt = item.TryGetProperty("updated_at", out var updatedAt) && updatedAt.ValueKind == JsonValueKind.String
                        ? updatedAt.GetDateTime()
                        : DateTime.UtcNow
                });
            }

            return result;
        }


    }
}