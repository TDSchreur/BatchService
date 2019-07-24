using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using Quartz.Spi;

namespace Worker
{
    public class ServiceWorker : IHostedService
    {
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;
        private readonly ILogger<ServiceWorker> _logger;
        private readonly ILogProvider _logProvider;
        private readonly ISchedulerFactory _schedulerFactory;

        private IScheduler _scheduler;

        public ServiceWorker(ISchedulerFactory schedulerFactory,
                             IJobFactory jobFactory,
                             IEnumerable<JobSchedule> jobSchedules,
                             ILogProvider logProvider,
                             ILogger<ServiceWorker> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobSchedules = jobSchedules;
            _logProvider = logProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            LogProvider.SetCurrentLogProvider(_logProvider);

            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            foreach (JobSchedule jobSchedule in _jobSchedules)
            {
                IJobDetail job = CreateJob(jobSchedule);
                ITrigger trigger = CreateTrigger(jobSchedule);

                _logger.LogInformation("Scheduling job {jobName} with cron {cron}", jobSchedule.JobType.Name, jobSchedule.CronExpression);

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
                                 .WithIdentity($"{schedule.JobType.FullName}.trigger")
                                 .WithCronSchedule(schedule.CronExpression)
                                 .WithDescription(schedule.CronExpression)
                                 .Build();
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            Type jobType = schedule.JobType;
            return JobBuilder.Create(jobType)
                             .WithIdentity(jobType.FullName)
                             .WithDescription(jobType.Name)
                             .Build();
        }
    }
}
