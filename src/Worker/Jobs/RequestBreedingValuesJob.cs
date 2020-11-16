using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class RequestBreedingValuesJob : IJob, IDisposable
    {
        private readonly ILogger<RequestBreedingValuesJob> _logger;
        private string _jobName = string.Empty;
        private CancellationToken _cancellationToken;

        public RequestBreedingValuesJob(ILogger<RequestBreedingValuesJob> logger)
        {
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

            _cancellationToken = context.CancellationToken;

            _jobName = context.JobDetail.Description ?? string.Empty;
            var lastRun = context.PreviousFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _logger.LogInformation("Greetings from {jobName}! Previous run: {lastRun}", _jobName, lastRun);
            
            _logger.LogInformation("Start work!!!");
            
            // Check to see if de job gets killed 20sec after cancellation is requested
            // See ServiceWorker.cs
            while(!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(2000, _cancellationToken);
                _logger.LogInformation($"Still working. Cancellation requested: {_cancellationToken.IsCancellationRequested}");
            }
            _logger.LogInformation("Work in done!!!");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogDebug("Disposing {jobName}", _jobName);
            }
        }
    }
}
