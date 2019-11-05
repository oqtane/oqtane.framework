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
    public class ScheduleLogController : Controller
    {
        private readonly IScheduleLogRepository ScheduleLogs;
        private readonly ILogManager logger;

        public ScheduleLogController(IScheduleLogRepository ScheduleLogs, ILogManager logger)
        {
            this.ScheduleLogs = ScheduleLogs;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public IEnumerable<ScheduleLog> Get()
        {
            return ScheduleLogs.GetScheduleLogs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public ScheduleLog Get(int id)
        {
            return ScheduleLogs.GetScheduleLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public ScheduleLog Post([FromBody] ScheduleLog ScheduleLog)
        {
            if (ModelState.IsValid)
            {
                ScheduleLog = ScheduleLogs.AddScheduleLog(ScheduleLog);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Schedule Log Added {ScheduleLog}", ScheduleLog);
            }
            return ScheduleLog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public ScheduleLog Put(int id, [FromBody] ScheduleLog ScheduleLog)
        {
            if (ModelState.IsValid)
            {
                ScheduleLog = ScheduleLogs.UpdateScheduleLog(ScheduleLog);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Schedule Log Updated {ScheduleLog}", ScheduleLog);
            }
            return ScheduleLog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            ScheduleLogs.DeleteScheduleLog(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Schedule Log Deleted {ScheduleLogId}", id);
        }
    }
}
