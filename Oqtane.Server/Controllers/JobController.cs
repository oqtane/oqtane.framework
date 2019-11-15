using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class JobController : Controller
    {
        private readonly IJobRepository Jobs;
        private readonly ILogManager logger;
        private readonly IServiceProvider ServiceProvider;

        public JobController(IJobRepository Jobs, ILogManager logger, IServiceProvider ServiceProvider)
        {
            this.Jobs = Jobs;
            this.logger = logger;
            this.ServiceProvider = ServiceProvider;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Job> Get()
        {
            return Jobs.GetJobs();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Job Get(int id)
        {
            return Jobs.GetJob(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public Job Post([FromBody] Job Job)
        {
            if (ModelState.IsValid)
            {
                Job = Jobs.AddJob(Job);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Job Added {Job}", Job);
            }
            return Job;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Job Put(int id, [FromBody] Job Job)
        {
            if (ModelState.IsValid)
            {
                Job = Jobs.UpdateJob(Job);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Job Updated {Job}", Job);
            }
            return Job;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            Jobs.DeleteJob(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Job Deleted {JobId}", id);
        }

        // GET api/<controller>/start
        [HttpGet("start/{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Start(int id)
        {
            Job job = Jobs.GetJob(id);
            Type jobtype = Type.GetType(job.JobType);
            if (jobtype != null)
            {
                var jobobject = ActivatorUtilities.CreateInstance(ServiceProvider, jobtype);
                ((IHostedService)jobobject).StartAsync(new System.Threading.CancellationToken());
            }
        }

        // GET api/<controller>/stop
        [HttpGet("stop/{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Stop(int id)
        {
            Job job = Jobs.GetJob(id);
            Type jobtype = Type.GetType(job.JobType);
            if (jobtype != null)
            {
                var jobobject = ActivatorUtilities.CreateInstance(ServiceProvider, jobtype);
                ((IHostedService)jobobject).StopAsync(new System.Threading.CancellationToken());
            }
        }
    }
}
