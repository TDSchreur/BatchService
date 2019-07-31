using System.Collections.Generic;
using Worker.Config;

namespace Worker
{
    public interface IJobSchedulesProvider
    {
        IReadOnlyList<JobSchedule> Jobs { get; }
    }
}
