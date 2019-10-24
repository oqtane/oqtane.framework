using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;

namespace Oqtane.Controllers
{

    [Route("{site}/api/[controller]")]
    public class LogController : Controller
    {
        private readonly ILogManager Logger;
        private readonly ILogRepository Logs;

        public LogController(ILogManager Logger, ILogRepository Logs)
        {
            this.Logger = Logger;
            this.Logs = Logs;
        }

        // GET: api/<controller>?siteid=x&level=y&function=z&rows=50
        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public IEnumerable<Log> Get(string siteid, string level, string function, string rows)
        {
            return Logs.GetLogs(int.Parse(siteid), level, function, int.Parse(rows));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Log Get(int id)
        {
            return Logs.GetLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] Log Log)
        {
            if (ModelState.IsValid)
            {
                Logger.Log(Log);
            }
        }
    }
}
