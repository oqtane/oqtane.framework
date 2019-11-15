using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class JobLogController : Controller
    {
        private readonly IJobLogRepository JobLogs;
        private readonly ILogManager logger;

        public JobLogController(IJobLogRepository JobLogs, ILogManager logger)
        {
            this.JobLogs = JobLogs;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public IEnumerable<JobLog> Get()
        {
            return JobLogs.GetJobLogs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Get(int id)
        {
            return JobLogs.GetJobLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Post([FromBody] JobLog JobLog)
        {
            if (ModelState.IsValid)
            {
                JobLog = JobLogs.AddJobLog(JobLog);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Job Log Added {JobLog}", JobLog);
            }
            return JobLog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Put(int id, [FromBody] JobLog JobLog)
        {
            if (ModelState.IsValid)
            {
                JobLog = JobLogs.UpdateJobLog(JobLog);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Job Log Updated {JobLog}", JobLog);
            }
            return JobLog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            JobLogs.DeleteJobLog(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Job Log Deleted {JobLogId}", id);
        }
    }
}
