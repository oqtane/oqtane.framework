using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{

    [Route("{site}/api/[controller]")]
    public class LogController : Controller
    {
        private readonly ILogManager _logger;
        private readonly ILogRepository _logs;

        public LogController(ILogManager logger, ILogRepository logs)
        {
            _logger = logger;
            _logs = logs;
        }

        // GET: api/<controller>?siteid=x&level=y&function=z&rows=50
        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public IEnumerable<Log> Get(string siteid, string level, string function, string rows)
        {
            return _logs.GetLogs(int.Parse(siteid), level, function, int.Parse(rows));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Log Get(int id)
        {
            return _logs.GetLog(id);
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] Log log)
        {
            if (ModelState.IsValid)
            {
                _logger.Log(log);
            }
        }
    }
}
