using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob, IDisposable
    {
        private readonly ILogger<HelloWorldJob> _logger;
        private string _jobName;

        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _jobName = context.JobDetail.Description;
            var lastRun = context.PreviousFireTimeUtc?.DateTime.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _logger.LogInformation("Greetings from {jobName}! Previous run: {lastRun}", _jobName, lastRun);

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogInformation("Disposing {jobName}", _jobName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
