using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class JobController : Controller
    {
        private readonly IJobRepository _jobs;
        private readonly ILogManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IStringLocalizer _localizer;

        public JobController(IJobRepository jobs, ILogManager logger, IServiceProvider serviceProvider, IStringLocalizer<JobController> localizer)
        {
            _jobs = jobs;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _localizer = localizer;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<Job> Get()
        {
            return _jobs.GetJobs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Job Get(int id)
        {
            return _jobs.GetJob(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public Job Post([FromBody] Job job)
        {
            if (ModelState.IsValid)
            {
                job = _jobs.AddJob(job);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, _localizer["Job Added {Job}"], job);
            }
            return job;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Job Put(int id, [FromBody] Job job)
        {
            if (ModelState.IsValid)
            {
                job = _jobs.UpdateJob(job);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, _localizer["Job Updated {Job}"], job);
            }
            return job;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            _jobs.DeleteJob(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Job Deleted {JobId}"], id);
        }

        // GET api/<controller>/start
        [HttpGet("start/{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Start(int id)
        {
            Job job = _jobs.GetJob(id);
            Type jobtype = Type.GetType(job.JobType);
            if (jobtype != null)
            {
                var jobobject = ActivatorUtilities.CreateInstance(_serviceProvider, jobtype);
                ((IHostedService)jobobject).StartAsync(new System.Threading.CancellationToken());
            }
        }

        // GET api/<controller>/stop
        [HttpGet("stop/{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Stop(int id)
        {
            Job job = _jobs.GetJob(id);
            Type jobtype = Type.GetType(job.JobType);
            if (jobtype != null)
            {
                var jobobject = ActivatorUtilities.CreateInstance(_serviceProvider, jobtype);
                ((IHostedService)jobobject).StopAsync(new System.Threading.CancellationToken());
            }
        }
    }
}
