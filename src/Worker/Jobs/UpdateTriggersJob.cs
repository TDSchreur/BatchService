using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;
using Worker.Core;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateTriggersJob : IJob
    {
        private readonly IJobSchedulesProvider _jobSchedulesProvider;
        private readonly ILogger<UpdateTriggersJob> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private CancellationToken _cancellationToken;

        public UpdateTriggersJob(ISchedulerFactory schedulerFactory,
                                IJobSchedulesProvider jobSchedulesProvider,
                                ILogger<UpdateTriggersJob> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedulesProvider = jobSchedulesProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _cancellationToken = context.CancellationToken;
            
            IScheduler scheduler = await _schedulerFactory.GetScheduler(_cancellationToken).ConfigureAwait(false);

            foreach (JobSchedule jobSchedule in _jobSchedulesProvider.Jobs)
            {
                TriggerKey triggerKey = new TriggerKey($"{jobSchedule.Type.FullName}.trigger");
                CronTriggerImpl trigger = await scheduler.GetTrigger(triggerKey, _cancellationToken).ConfigureAwait(false) as CronTriggerImpl;

                if (trigger != null && 
                    trigger.CronExpressionString == jobSchedule.Cron)
                {
                    continue;
                }

                _logger.LogInformation("Updating trigger {triggerName} from {oldCron} to {jobCron}", triggerKey.Name, trigger.CronExpressionString, jobSchedule.Cron);

                trigger.CronExpressionString = jobSchedule.Cron;

                await scheduler.RescheduleJob(triggerKey, trigger, _cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
