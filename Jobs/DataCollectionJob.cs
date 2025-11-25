using CoinUpWorkerService.Data;
using CoinUpWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUpWorkerService.Jobs
{
    public class DataCollectionJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataCollectionJob> _logger;
        public DataCollectionJob(IServiceProvider serviceProvider, ILogger<DataCollectionJob> logger)

        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Job de collecte démarré à {Time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var collector = scope.ServiceProvider.GetRequiredService<IDataCollectorService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


                // -----------------------------
                // 1. Fetch external data
                // -----------------------------
                var coinsMarkets = await collector.FetchCoinsMarketAsync();
                var marketCategories = await collector.FetchMarketCategoriesAsync();


                // -----------------------------
                // 2. DELETE all existing DB rows 
                // -----------------------------
                dbContext.CoinsMarket.RemoveRange(dbContext.CoinsMarket);
                dbContext.CoinsMarketCategory.RemoveRange(dbContext.CoinsMarketCategory);
                dbContext.MarketChartDetails.RemoveRange(dbContext.MarketChartDetails);

                await dbContext.SaveChangesAsync(); // Important to clear table before insert


                // -----------------------------
                // 3. INSERT new rows
                // -----------------------------
                await dbContext.CoinsMarket.AddRangeAsync(coinsMarkets);
                await dbContext.CoinsMarketCategory.AddRangeAsync(marketCategories);

                await dbContext.SaveChangesAsync();


                // -----------------------------
                // 4. Fetch Market Chart For Each Coin
                // -----------------------------
                _logger.LogInformation("Fetching market chart for {Count} coins...", coinsMarkets.Count);
                var rateLimitMs = 2000;
                foreach (var coin in coinsMarkets.Take(5))
                {
                    try
                    {
                        var chart = await collector.FetchMarketChartAsync(coin.Id);

                        if (chart == null)
                        {
                            _logger.LogWarning("Market chart is null for coin {Id}", coin.Id);
                            continue;
                        }

                        // TODO: Save chart in DB
                        await dbContext.MarketChartDetails.AddRangeAsync(chart);

                        _logger.LogInformation("Chart fetched for {Id}", coin.Id);
                    }
                    catch (HttpRequestException ex) when ((int?)ex.StatusCode == 429)
                    {
                        _logger.LogWarning("Rate limit hit. Waiting 10s before retrying...");
                        await Task.Delay(10000);
                    }

                    // Delay between requests to avoid rate limiting
                    await Task.Delay(rateLimitMs);
                }


                // If you have a MarketCharts table
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("🟢 Job terminé avec succès !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Erreur dans le job de collecte");
            }
        }

    }
}
