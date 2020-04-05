using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class JobLogController : Controller
    {
        private readonly IJobLogRepository _jobLogs;
        private readonly ILogManager _logger;

        public JobLogController(IJobLogRepository jobLogs, ILogManager logger)
        {
            _jobLogs = jobLogs;
            _logger = logger;
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
        public JobLog Post([FromBody] JobLog jobLog)
        {
            if (ModelState.IsValid)
            {
                jobLog = _jobLogs.AddJobLog(jobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Job Log Added {JobLog}", jobLog);
            }
            return jobLog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public JobLog Put(int id, [FromBody] JobLog jobLog)
        {
            if (ModelState.IsValid)
            {
                jobLog = _jobLogs.UpdateJobLog(jobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Job Log Updated {JobLog}", jobLog);
            }
            return jobLog;
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
