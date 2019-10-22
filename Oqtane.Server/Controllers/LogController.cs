using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Repository;
using Oqtane.Infrastructure;

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

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Log> Get(string siteid)
        {
            return Logs.GetLogs(int.Parse(siteid));
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] Log Log)
        {
            if (ModelState.IsValid)
            {
                Logger.AddLog(Log);
            }
        }
    }
}
