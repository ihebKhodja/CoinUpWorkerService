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

                await dbContext.SaveChangesAsync(); // Important to clear table before insert


                // -----------------------------
                // 3. INSERT new rows
                // -----------------------------
                await dbContext.CoinsMarket.AddRangeAsync(coinsMarkets);
                await dbContext.CoinsMarketCategory.AddRangeAsync(marketCategories);

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
