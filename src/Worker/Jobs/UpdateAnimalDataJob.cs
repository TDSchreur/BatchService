//using System;
//using System.Globalization;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Quartz;

//namespace Worker.Jobs
//{
//    [DisallowConcurrentExecution]
//    public class UpdateAnimalDataJob : IJob, IDisposable
//    {
//        private readonly ILogger<UpdateAnimalDataJob> _logger;
//        private string _jobName;
//        private CancellationToken _cancellationToken;

//        public UpdateAnimalDataJob(ILogger<UpdateAnimalDataJob> logger)
//        {
//            _logger = logger;
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        public Task Execute(IJobExecutionContext context)
//        {
//            _cancellationToken = context.CancellationToken;

//            _jobName = context.JobDetail.Description;
//            string lastRun = context.PreviousFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.InvariantCulture) ?? string.Empty;
//            _logger.LogInformation("Greetings from {jobName}! Previous run: {lastRun}", _jobName, lastRun);

//            return Task.CompletedTask;
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _logger.LogDebug("Disposing {jobName}", _jobName);
//            }
//        }
//    }
//}
