using System;
using System.Collections.Generic;

namespace Worker.Core
{
    public class JobSchedules
    {
#nullable disable
        public IReadOnlyList<JobSchedule> Jobs { get; set; }
#nullable restore
    }
}
