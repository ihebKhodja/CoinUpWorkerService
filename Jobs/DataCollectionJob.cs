using CoinUpWorkerService.Data;
using CoinUpWorkerService.Services;
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

                var data = await collector.FetchMarketDataAsync();

                foreach (var item in data)
                {
                    dbContext.MarketData.Add(item);
                }

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Données sauvegardées : {Count} objets", data.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans le job de collecte");
            }
        }
    }
}
