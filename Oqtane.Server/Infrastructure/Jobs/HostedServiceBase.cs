using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public abstract class HostedServiceBase : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HostedServiceBase(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        // abstract method must be overridden
        public abstract string ExecuteJob(IServiceProvider provider);

        // public properties which can be overridden and are used during auto registration of job
        public string Name { get; set; } = "";
        public string Frequency { get; set; } = "d"; // day
        public int Interval { get; set; } = 1;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public int RetentionHistory { get; set; } = 10;
        public bool IsEnabled { get; set; } = false;

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // required so that this method does not block startup

            try
            {                
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // get name of job
                        string jobType = Utilities.GetFullTypeName(GetType().AssemblyQualifiedName);

                        // load jobs and find current job
                        IJobRepository jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                        Job job = jobs.GetJobs().Where(item => item.JobType == jobType).FirstOrDefault();
                        if (job != null && job.IsEnabled && !job.IsExecuting)
                        {
                            // get next execution date
                            DateTime NextExecution;
                            if (job.NextExecution == null)
                            {
                                if (job.StartDate != null)
                                {
                                    NextExecution = job.StartDate.Value;
                                }
                                else
                                {
                                    NextExecution = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                NextExecution = job.NextExecution.Value;
                            }

                            // determine if the job should be run
                            if (NextExecution <= DateTime.UtcNow && (job.EndDate == null || job.EndDate >= DateTime.UtcNow))
                            {
                                IJobLogRepository jobLogs = scope.ServiceProvider.GetRequiredService<IJobLogRepository>();

                                // create a job log entry
                                JobLog log = new JobLog();
                                log.JobId = job.JobId;
                                log.StartDate = DateTime.UtcNow;
                                log.FinishDate = null;
                                log.Succeeded = false;
                                log.Notes = "";
                                log = jobLogs.AddJobLog(log);

                                // update the job to indicate it is running
                                job.IsExecuting = true;
                                jobs.UpdateJob(job);

                                // execute the job
                                try
                                {
                                    var notes = "";
                                    var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                                    var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
                                    foreach (var tenant in tenantRepository.GetTenants())
                                    {
                                        // set tenant and execute job
                                        tenantManager.SetTenant(tenant.TenantId);
                                        notes += ExecuteJob(scope.ServiceProvider);
                                    }
                                    log.Notes = notes;
                                    log.Succeeded = true;
                                }
                                catch (Exception ex)
                                {
                                    log.Notes = ex.Message;
                                    log.Succeeded = false;
                                }

                                // update the job log
                                log.FinishDate = DateTime.UtcNow;
                                jobLogs.UpdateJobLog(log);

                                // update the job
                                job.NextExecution = CalculateNextExecution(NextExecution, job);
                                if (job.Frequency == "O") // one time
                                {
                                    job.EndDate = DateTime.UtcNow;
                                    job.NextExecution = null;
                                }
                                job.IsExecuting = false;
                                jobs.UpdateJob(job);

                                // trim the job log
                                List<JobLog> logs = jobLogs.GetJobLogs().Where(item => item.JobId == job.JobId)
                                    .OrderByDescending(item => item.JobLogId).ToList();
                                for (int i = logs.Count; i > job.RetentionHistory; i--)
                                {
                                    jobLogs.DeleteJobLog(logs[i - 1].JobLogId);
                                }
                            }
                        }
                    }

                    // wait 1 minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch
            {
                // can occur during the initial installation as there is no DBContext
            }

        }

        private DateTime CalculateNextExecution(DateTime nextExecution, Job job)
        {
            switch (job.Frequency)
            {
                case "m": // minutes
                    nextExecution = nextExecution.AddMinutes(job.Interval);
                    break;
                case "H": // hours
                    nextExecution = nextExecution.AddHours(job.Interval);
                    break;
                case "d": // days
                    nextExecution = nextExecution.AddDays(job.Interval);
                    if (job.StartDate != null && job.StartDate.Value.TimeOfDay.TotalSeconds != 0)
                    {
                        // set the start time
                        nextExecution = nextExecution.Date.Add(job.StartDate.Value.TimeOfDay);
                    }
                    break;
                case "w": // weeks
                    nextExecution = nextExecution.AddDays(job.Interval * 7);
                    if (job.StartDate != null && job.StartDate.Value.TimeOfDay.TotalSeconds != 0)
                    {
                        // set the start time
                        nextExecution = nextExecution.Date.Add(job.StartDate.Value.TimeOfDay);
                    }
                    break;
                case "M": // months
                    nextExecution = nextExecution.AddMonths(job.Interval);
                    if (job.StartDate != null && job.StartDate.Value.TimeOfDay.TotalSeconds != 0)
                    {
                        // set the start time
                        nextExecution = nextExecution.Date.Add(job.StartDate.Value.TimeOfDay);
                    }
                    break;
                case "O": // one time
                    break;
            }
            if (nextExecution < DateTime.UtcNow)
            {
                nextExecution = DateTime.UtcNow;
            }
            return nextExecution;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    string jobTypeName = Utilities.GetFullTypeName(GetType().AssemblyQualifiedName);
                    IJobRepository jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    Job job = jobs.GetJobs().Where(item => item.JobType == jobTypeName).FirstOrDefault();
                    if (job != null)
                    {
                        // reset in case this job was forcefully terminated previously 
                        job.IsStarted = true;
                        job.IsExecuting = false;
                        jobs.UpdateJob(job);
                    }
                    else
                    {
                        // auto registration - job will not run on initial installation but will run after restart
                        job = new Job { JobType = jobTypeName };

                        // optional HostedServiceBase properties
                        var jobType = Type.GetType(jobTypeName);
                        var jobObject = ActivatorUtilities.CreateInstance(scope.ServiceProvider, jobType) as HostedServiceBase;
                        if (jobObject.Name != "")
                        {
                            job.Name = jobObject.Name;
                        }
                        else
                        {
                            job.Name = Utilities.GetTypeName(job.JobType);
                        }
                        job.Frequency = jobObject.Frequency;
                        job.Interval = jobObject.Interval;
                        job.StartDate = jobObject.StartDate;
                        job.EndDate = jobObject.EndDate;
                        job.RetentionHistory = jobObject.RetentionHistory;
                        job.IsEnabled = jobObject.IsEnabled;
                        job.IsStarted = true;
                        job.IsExecuting = false;
                        job.NextExecution = null;
                        jobs.AddJob(job);
                    }
                }

                _executingTask = ExecuteAsync(_cancellationTokenSource.Token);

                if (_executingTask.IsCompleted)
                {
                    return _executingTask;
                }
            }
            catch
            {
                // can occur during the initial installation because this method is called during startup and the database has not yet been created
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    string jobTypeName = Utilities.GetFullTypeName(GetType().AssemblyQualifiedName);
                    IJobRepository jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    Job job = jobs.GetJobs().Where(item => item.JobType == jobTypeName).FirstOrDefault();
                    if (job != null)
                    {
                        // reset job 
                        job.IsStarted = false;
                        job.IsExecuting = false;
                        jobs.UpdateJob(job);
                    }
                }
            }
            catch
            {
                // error updating the job
            }

            // stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // force cancellation of the executing method
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                // wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
