using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Worker.Config;

namespace Worker
{
    public class ServiceWorker : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<ServiceWorker> _logger;
        private readonly JobSchedules _jobSchedules;

        private IScheduler _scheduler;

        public ServiceWorker(ISchedulerFactory schedulerFactory,
                             IOptionsMonitor<JobSchedules> jobConfigs,
                             ILogger<ServiceWorker> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobConfigs.CurrentValue;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken); 

            foreach (var jobSchedule in _jobSchedules.Jobs)
            {
                IJobDetail job = CreateJob(jobSchedule);
                ITrigger trigger = CreateTrigger(jobSchedule);

                _logger.LogInformation("Scheduling job {jobName} with cron {cron}", jobSchedule.Type.Name, jobSchedule.Cron);

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown(cancellationToken);
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
