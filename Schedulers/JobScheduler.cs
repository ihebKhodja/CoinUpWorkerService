using CoinUpWorkerService.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUpWorkerService.Schedulers
{
    public class JobScheduler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobScheduler> _logger;

        public JobScheduler(IServiceProvider serviceProvider, ILogger<JobScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ScheduleDataCollectionJob(TimeSpan interval)
        {
            var timer = new PeriodicTimer(interval);

            while (await timer.WaitForNextTickAsync())
            {
                //using var scope = _serviceProvider.CreateScope();
                //var job = scope.ServiceProvider.GetRequiredService<DataCollectionJob>();
                //await job.ExecuteAsync();
                await ScheduleDataCollectionJobOnce();

            }
        }
        public async Task ScheduleDataCollectionJobOnce()
        {
            using var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<DataCollectionJob>();
            await job.ExecuteAsync();
        }
    }
}
