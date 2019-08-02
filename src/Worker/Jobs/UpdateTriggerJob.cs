using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;
using Worker.Core;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateTriggerJob : IJob
    {
        private readonly IJobSchedulesProvider _jobSchedulesProvider;
        private readonly ILogger<UpdateTriggerJob> _logger;
        private readonly ISchedulerFactory _schedulerFactory;

        public UpdateTriggerJob(ISchedulerFactory schedulerFactory,
                                IJobSchedulesProvider jobSchedulesProvider,
                                ILogger<UpdateTriggerJob> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedulesProvider = jobSchedulesProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            IScheduler scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);

            foreach (JobSchedule jobSchedule in _jobSchedulesProvider.Jobs)
            {
                TriggerKey triggerKey = new TriggerKey($"{jobSchedule.Type.FullName}.trigger");
                CronTriggerImpl trigger = await scheduler.GetTrigger(triggerKey).ConfigureAwait(false) as CronTriggerImpl;

                if (trigger.CronExpressionString == jobSchedule.Cron)
                {
                    continue;
                }

                _logger.LogInformation("Updating trigger {triggerName} from {oldCron} to {jobCron}", triggerKey.Name, trigger.CronExpressionString, jobSchedule.Cron);

                trigger.CronExpressionString = jobSchedule.Cron;

                await scheduler.RescheduleJob(triggerKey, trigger).ConfigureAwait(false);
            }
        }
    }
}
