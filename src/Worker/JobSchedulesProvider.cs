using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Worker.Config;

namespace Worker
{
    public class JobSchedulesProvider : IJobSchedulesProvider
    {
        private readonly JobSchedules _jobSchedules;
        public JobSchedulesProvider(IOptionsMonitor<JobSchedules> jobConfig)
        {
            if (jobConfig is null)
            {
                throw new System.ArgumentNullException(nameof(jobConfig));
            }

            _jobSchedules = jobConfig.CurrentValue;
        }

        public IReadOnlyList<JobSchedule> Jobs => _jobSchedules.Jobs;
    }
}
