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

            var interval = TimeSpan.FromMinutes(5); // Configure interval here
            var timer = new PeriodicTimer(interval);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("⏳ Lancement de la collecte à {Time}", DateTimeOffset.Now);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();

                    await scheduler.ScheduleDataCollectionJobOnce(); // run job
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors de la collecte des données");
                }
            }

            _logger.LogWarning("⚠️ Worker arrêté suite à un signal d’annulation.");
        }
    }
}
