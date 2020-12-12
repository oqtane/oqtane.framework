using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class JobLogController : Controller
    {
        private readonly IJobLogRepository _jobLogs;
        private readonly ILogManager _logger;
        private readonly IStringLocalizer _localizer;

        public JobLogController(IJobLogRepository jobLogs, ILogManager logger, IStringLocalizer<JobLogController> localizer)
        {
            _jobLogs = jobLogs;
            _logger = logger;
            _localizer = localizer;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<JobLog> Get()
        {
            return _jobLogs.GetJobLogs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public JobLog Get(int id)
        {
            return _jobLogs.GetJobLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public JobLog Post([FromBody] JobLog jobLog)
        {
            if (ModelState.IsValid)
            {
                jobLog = _jobLogs.AddJobLog(jobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, _localizer["Job Log Added {JobLog}"], jobLog);
            }
            return jobLog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public JobLog Put(int id, [FromBody] JobLog jobLog)
        {
            if (ModelState.IsValid)
            {
                jobLog = _jobLogs.UpdateJobLog(jobLog);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, _localizer["Job Log Updated {JobLog}"], jobLog);
            }
            return jobLog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            _jobLogs.DeleteJobLog(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Job Log Deleted {JobLogId}"], id);
        }
    }
}
