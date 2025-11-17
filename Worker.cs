using CoinUpWorkerService.Schedulers;

namespace CoinUpWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker démarré à {Time}", DateTimeOffset.Now);

            var timer = new PeriodicTimer(TimeSpan.FromMinutes(1)); // ⬅️ Intervalle configurable

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Collecte des données à {Time}", DateTimeOffset.Now);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();
                    await scheduler.ScheduleDataCollectionJobOnce(); // ⬅️ Appelle le scheduler
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la collecte des données");
                }
            }
        }
    }
}