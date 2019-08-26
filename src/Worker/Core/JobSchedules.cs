using System.Collections.Generic;

namespace Worker.Core
{
    public class JobSchedules
    {
        public IReadOnlyList<JobSchedule> Jobs { get; set; }
    }
}
