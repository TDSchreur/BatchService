using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Worker.Core
{
    public class JobSchedulesProvider : IJobSchedulesProvider
    {
        private readonly JobSchedules _jobSchedules;

        public JobSchedulesProvider(IOptions<JobSchedules> jobConfig)
        {
            if (jobConfig is null)
            {
                throw new ArgumentNullException(nameof(jobConfig));
            }

            _jobSchedules = jobConfig.Value;
        }

        public IReadOnlyList<JobSchedule> Jobs => _jobSchedules.Jobs;
    }
}
