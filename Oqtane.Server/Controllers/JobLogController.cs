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
        private readonly IJobLogRepository _jobLogs;
        private readonly ILogManager _logger;

        public JobLogController(IJobLogRepository JobLogs, ILogManager logger)
        {
            this._jobLogs = JobLogs;
            this._logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public IEnumerable<JobLog> Get()
        {
            return _jobLogs.GetJobLogs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Get(int id)
        {
            return _jobLogs.GetJobLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Post([FromBody] JobLog JobLog)
        {
            if (ModelState.IsValid)
            {
                JobLog = _jobLogs.AddJobLog(JobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Job Log Added {JobLog}", JobLog);
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
                JobLog = _jobLogs.UpdateJobLog(JobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Job Log Updated {JobLog}", JobLog);
            }
            return JobLog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            _jobLogs.DeleteJobLog(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Job Log Deleted {JobLogId}", id);
        }
    }
}
