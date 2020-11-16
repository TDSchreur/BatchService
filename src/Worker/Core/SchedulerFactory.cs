using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;

namespace Worker.Core
{
    public class SchedulerFactory : StdSchedulerFactory
    {
        private readonly IJobFactory _jobFactory;
        private readonly ILogProvider _logProvider;
        private IScheduler _scheduler = default!;

        public SchedulerFactory(ILogProvider logProvider, IJobFactory jobFactory)
        {
            _logProvider = logProvider;
            _jobFactory = jobFactory;
        }

        public override async Task<IScheduler> GetScheduler(CancellationToken cancellationToken = default)
        {
            if (_scheduler == null)
            {
                _scheduler = await base.GetScheduler(cancellationToken).ConfigureAwait(false);
                LogProvider.SetCurrentLogProvider(_logProvider);
                _scheduler.JobFactory = _jobFactory;
            }

            return _scheduler;
        }
    }
}
