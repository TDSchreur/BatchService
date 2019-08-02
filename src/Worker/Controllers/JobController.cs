using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Worker.Core;
using Worker.Jobs;

namespace Worker.Controllers
{
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobSchedulesProvider _jobSchedulesProvider;
        private readonly ISchedulerFactory _schedulerFactory;

        public JobController(
            ISchedulerFactory schedulerFactory,
            IJobSchedulesProvider jobSchedulesProvider)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedulesProvider = jobSchedulesProvider;
        }

        [HttpGet]
        public ActionResult<IEnumerable<JobModel>> Get()
        {
            List<JobModel> jobs = _jobSchedulesProvider.Jobs
                                                       .Where(x => x.Type != typeof(UpdateTriggerJob))
                                                       .Select(x => new JobModel {Name = x.Name, Cron = x.Cron})
                                                       .ToList();

            return Ok(jobs);
        }

        [HttpGet("start/{jobName}")]
        public async Task<ActionResult> Start(string jobName)
        {
            JobSchedule job = _jobSchedulesProvider.Jobs.FirstOrDefault(x => x.Name == jobName);

            if (job == null)
            {
                return NotFound();
            }

            IScheduler scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);

            JobKey jobKey = new JobKey(job.Type.FullName);
            await scheduler.TriggerJob(jobKey).ConfigureAwait(false);

            return Ok();
        }

        public class JobModel
        {
            public string Name { get; set; }

            public string Cron { get; set; }
        }
    }
}
