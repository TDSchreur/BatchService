using System.Collections.Generic;
using Worker.Config;

namespace Worker
{
    public interface IJobSchedulesProvider
    {
        IEnumerable<JobSchedule> Jobs { get; }
    }
}
