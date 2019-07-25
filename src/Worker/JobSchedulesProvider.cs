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
            _jobSchedules = jobConfig.CurrentValue;
        }

        public IEnumerable<JobSchedule> Jobs => _jobSchedules.Jobs;
    }
}