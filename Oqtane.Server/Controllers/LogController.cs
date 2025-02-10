using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Enums;
using System.Net;

namespace Oqtane.Controllers
{

    [Route(ControllerRoutes.ApiRoute)]
    public class LogController : Controller
    {
        private readonly ILogManager _logger;
        private readonly ILogRepository _logs;
        private readonly Alias _alias;

        public LogController(ILogManager logger, ILogRepository logs, ITenantManager tenantManager)
        {
            _logger = logger;
            _logs = logs;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&level=y&function=z&rows=50
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<Log> Get(string siteid, string level, string function, string rows)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                return _logs.GetLogs(SiteId, level, function, SharedConverter.ParseInteger(rows));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Log Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }

        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Log Get(int id)
        {
            var log = _logs.GetLog(id);
            if (log != null && log.SiteId == _alias.SiteId)
            {
                return log;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Log Get Attempt {LogId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] Log log)
        {
            if (ModelState.IsValid && log.SiteId == _alias.SiteId)
            {
                _logger.Log(log);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Log Post Attempt {Log}", log);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [HttpDelete]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(string siteId)
        {
            if (int.TryParse(siteId, out int parsedSiteId) && parsedSiteId == _alias.SiteId)
            {
                _logs.DeleteLogs(parsedSiteId, 0); // specifying zero for age results in all logs being deleted
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Log Delete Attempt {SiteId}", siteId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
