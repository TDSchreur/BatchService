using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Worker.Core;

namespace Worker
{
    public class ServiceWorker : IHostedService
    {
        private readonly IJobSchedulesProvider _jobSchedulesProvider;
        private readonly ILogger<ServiceWorker> _logger;
        private readonly ISchedulerFactory _schedulerFactory;

        private IScheduler _scheduler;

        public ServiceWorker(ISchedulerFactory schedulerFactory,
                             IJobSchedulesProvider jobSchedulesProvider,
                             ILogger<ServiceWorker> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedulesProvider = jobSchedulesProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);

            foreach (JobSchedule jobSchedule in _jobSchedulesProvider.Jobs)
            {
                IJobDetail job = CreateJob(jobSchedule);
                ITrigger trigger = CreateTrigger(jobSchedule);

                _logger.LogInformation("Scheduling job {jobKey} with cron {cron}", job.Key, jobSchedule.Cron);

                await _scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);
            }

            await _scheduler.Start(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null)
            {
                var runningJobs = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken);

                var tasks = runningJobs.Select(job => _scheduler.Interrupt(job.JobDetail.Key, cancellationToken))
                                       .Cast<Task>()
                                       .ToList();

                await Task.WhenAll(tasks);

                var shutdown = _scheduler.Shutdown(true, cancellationToken);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await Task.WhenAny(shutdown, Task.Delay(Timeout.Infinite, cts.Token));
            }
        }

        private static ITrigger CreateTrigger(JobSchedule schedule)
        {
            return TriggerBuilder.Create()
                                 .WithIdentity($"{schedule.Type.FullName}.trigger")
                                 .WithCronSchedule(schedule.Cron)
                                 .WithDescription(schedule.Cron)
                                 .Build();
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            Type jobType = schedule.Type;
            return JobBuilder.Create(jobType)
                             .WithIdentity(jobType.FullName)
                             .WithDescription(jobType.Name)
                             .Build();
        }
    }
}
