using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;

namespace Worker
{
    public class SchedulerFactory : StdSchedulerFactory
    {
        private readonly ILogProvider _logProvider;
        private readonly IJobFactory _jobFactory;
        private IScheduler _scheduler;

        public SchedulerFactory(ILogProvider logProvider, IJobFactory jobFactory)
        {
            _logProvider = logProvider;
            _jobFactory = jobFactory;
        }
        public override Task<IReadOnlyList<IScheduler>> GetAllSchedulers(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
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

        public override Task<IScheduler> GetScheduler(string schedName, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
