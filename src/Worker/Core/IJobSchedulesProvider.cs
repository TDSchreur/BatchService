using System;
using System.Collections.Generic;

namespace Worker.Core
{
    public interface IJobSchedulesProvider
    {
        IReadOnlyList<JobSchedule> Jobs { get; }
    }
}
