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
    public class JobTaskController : Controller
    {
        private readonly IJobTaskRepository _jobTasks;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public JobTaskController(IJobTaskRepository jobTasks, ILogManager logger, ITenantManager tenantManager)
        {
            _jobTasks = jobTasks;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public JobTask Get(int id)
        {
            var jobTask = _jobTasks.GetJobTask(id);
            if (jobTask.SiteId == _alias.SiteId)
            {
                return jobTask;
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
        public JobTask Post([FromBody] JobTask jobTask)
        {
            if (ModelState.IsValid && jobTask.SiteId == _alias.SiteId)
            {
                jobTask.IsCompleted = false;
                jobTask = _jobTasks.AddJobTask(jobTask);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Job Task Added {JobTask}", jobTask);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Job Task Post Attempt {JobTask}", jobTask);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                jobTask = null;
            }
            return jobTask;
        }
    }
}
