using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Worker.Core
{
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzJobRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using IServiceScope scope = _serviceProvider.CreateScope();
            if (scope.ServiceProvider.GetRequiredService(context.JobDetail.JobType) is IJob job)
            {
                await job.Execute(context).ConfigureAwait(false);
            }
        }
    }
}
