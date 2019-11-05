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
    public class ScheduleController : Controller
    {
        private readonly IScheduleRepository Schedules;
        private readonly ILogManager logger;

        public ScheduleController(IScheduleRepository Schedules, ILogManager logger)
        {
            this.Schedules = Schedules;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Schedule> Get()
        {
            return Schedules.GetSchedules();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Schedule Get(int id)
        {
            return Schedules.GetSchedule(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public Schedule Post([FromBody] Schedule Schedule)
        {
            if (ModelState.IsValid)
            {
                Schedule = Schedules.AddSchedule(Schedule);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Schedule Added {Schedule}", Schedule);
            }
            return Schedule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Schedule Put(int id, [FromBody] Schedule Schedule)
        {
            if (ModelState.IsValid)
            {
                Schedule = Schedules.UpdateSchedule(Schedule);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Schedule Updated {Schedule}", Schedule);
            }
            return Schedule;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            Schedules.DeleteSchedule(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Schedule Deleted {ScheduleId}", id);
        }
    }
}
