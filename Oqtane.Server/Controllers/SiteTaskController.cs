using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SiteTaskController : Controller
    {
        private readonly ISiteTaskRepository _siteTasks;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SiteTaskController(ISiteTaskRepository siteTasks, ILogManager logger, ITenantManager tenantManager)
        {
            _siteTasks = siteTasks;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public SiteTask Get(int id)
        {
            var siteTask = _siteTasks.GetSiteTask(id);
            if (siteTask.SiteId == _alias.SiteId)
            {
                return siteTask;
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public SiteTask Post([FromBody] SiteTask siteTask)
        {
            if (ModelState.IsValid && siteTask.SiteId == _alias.SiteId)
            {
                siteTask.IsCompleted = false;
                siteTask = _siteTasks.AddSiteTask(siteTask);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Site Task Added {SiteTask}", siteTask);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Task Post Attempt {SiteTask}", siteTask);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteTask = null;
            }
            return siteTask;
        }
    }
}
