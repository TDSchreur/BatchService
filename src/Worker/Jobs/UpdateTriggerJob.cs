using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;
using Worker.Config;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateTriggerJob : IJob
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobSchedulesProvider _jobSchedulesProvider;
        private readonly ILogger<UpdateTriggerJob> _logger;

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
            var scheduler = await _schedulerFactory.GetScheduler();

            foreach (var jobSchedule in _jobSchedulesProvider.Jobs)
            {
                var triggerKey = new TriggerKey($"{jobSchedule.Type.FullName}.trigger");
                var trigger = await scheduler.GetTrigger(triggerKey) as CronTriggerImpl;

                if (trigger.CronExpressionString == jobSchedule.Cron)
                    continue;

                _logger.LogInformation("Updating tigger {triggerName} from {oldCron} to {jobCron}", triggerKey.Name, trigger.CronExpressionString, jobSchedule.Cron);

                trigger.CronExpressionString = jobSchedule.Cron;

                await scheduler.RescheduleJob(triggerKey, trigger);
            }
        }
    }
}
