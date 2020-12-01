using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class MakeSnapshotJob : IJob, IDisposable
    {
        private readonly ILogger<MakeSnapshotJob> _logger;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IMakeSnapshotServiceAgent _makeSnapshotServiceAgent;
        private CancellationTokenSource _cts = default!;
        private string _jobName = string.Empty;

        public MakeSnapshotJob(
            IHostEnvironment hostingEnvironment,
            IMakeSnapshotServiceAgent makeSnapshotServiceAgent,
            ILogger<MakeSnapshotJob> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _makeSnapshotServiceAgent = makeSnapshotServiceAgent;
            _logger = logger;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);

            _jobName = context.JobDetail.Description ?? string.Empty;
            var lastRun = context.PreviousFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _logger.LogInformation("Starting {jobName}! Previous run: {lastRun}", _jobName, lastRun);

            await _makeSnapshotServiceAgent.MakeSnapshot();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogDebug("Disposing {jobName}", _jobName);
                _cts?.Dispose();
            }
        }
    }
}
