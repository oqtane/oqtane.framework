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
        private Task ExecutingTask;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private readonly IServiceScopeFactory ServiceScopeFactory;

        public HostedServiceBase(IServiceScopeFactory ServiceScopeFactory)
        {
            this.ServiceScopeFactory = ServiceScopeFactory;
        }

        // abstract method must be overridden
        public abstract string ExecuteJob(IServiceProvider provider);

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        // get name of job
                        string JobType = Utilities.GetFullTypeName(this.GetType().AssemblyQualifiedName);

                        // load jobs and find current job
                        IJobRepository Jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                        Job Job = Jobs.GetJobs().Where(item => item.JobType == JobType).FirstOrDefault();
                        if (Job != null && Job.IsEnabled && !Job.IsExecuting)
                        {
                            // set next execution date
                            if (Job.NextExecution == null)
                            {
                                if (Job.StartDate != null)
                                {
                                    Job.NextExecution = Job.StartDate;
                                }
                                else
                                {
                                    Job.NextExecution = DateTime.Now;
                                }
                            }

                            // determine if the job should be run
                            if (Job.NextExecution <= DateTime.Now && (Job.EndDate == null || Job.EndDate >= DateTime.Now))
                            {
                                IJobLogRepository JobLogs = scope.ServiceProvider.GetRequiredService<IJobLogRepository>();

                                // create a job log entry
                                JobLog log = new JobLog();
                                log.JobId = Job.JobId;
                                log.StartDate = DateTime.Now;
                                log.FinishDate = null;
                                log.Succeeded = false;
                                log.Notes = "";
                                log = JobLogs.AddJobLog(log);

                                // update the job to indicate it is running
                                Job.IsExecuting = true;
                                Jobs.UpdateJob(Job);

                                // execute the job
                                try
                                {
                                    log.Notes = ExecuteJob(scope.ServiceProvider);
                                    log.Succeeded = true;
                                }
                                catch (Exception ex)
                                {
                                    log.Notes = ex.Message;
                                    log.Succeeded = false;
                                }

                                // update the job log
                                log.FinishDate = DateTime.Now;
                                JobLogs.UpdateJobLog(log);

                                // update the job
                                Job.NextExecution = CalculateNextExecution(Job.NextExecution.Value, Job.Frequency, Job.Interval);
                                Job.IsExecuting = false;
                                Jobs.UpdateJob(Job);

                                // trim the job log
                                List<JobLog> logs = JobLogs.GetJobLogs().Where(item => item.JobId == Job.JobId)
                                    .OrderByDescending(item => item.JobLogId).ToList();
                                for (int i = logs.Count; i > Job.RetentionHistory; i--)
                                {
                                    JobLogs.DeleteJobLog(logs[i - 1].JobLogId);
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

        private DateTime CalculateNextExecution(DateTime NextExecution, string Frequency, int Interval)
        {
            switch (Frequency)
            {
                case "m": // minutes
                    NextExecution = NextExecution.AddMinutes(Interval);
                    break;
                case "H": // hours
                    NextExecution = NextExecution.AddHours(Interval);
                    break;
                case "d": // days
                    NextExecution = NextExecution.AddDays(Interval);
                    break;
                case "M": // months
                    NextExecution = NextExecution.AddMonths(Interval);
                    break;
            }
            if (NextExecution < DateTime.Now)
            {
                NextExecution = DateTime.Now;
            }
            return NextExecution;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // set IsExecuting to false in case this job was forcefully terminated previously 
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    string JobType = Utilities.GetFullTypeName(this.GetType().AssemblyQualifiedName);
                    IJobRepository Jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    Job Job = Jobs.GetJobs().Where(item => item.JobType == JobType).FirstOrDefault();
                    if (Job != null)
                    {
                        Job.IsStarted = true;
                        Job.IsExecuting = false;
                        Jobs.UpdateJob(Job);
                    }
                }
            }
            catch
            {
                // can occur during the initial installation as there is no DBContext
            }

            ExecutingTask = ExecuteAsync(CancellationTokenSource.Token);

            if (ExecutingTask.IsCompleted)
            {
                return ExecutingTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken CancellationToken)
        {
            if (ExecutingTask == null)
            {
                return;
            }

            try
            {
                CancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(ExecutingTask, Task.Delay(Timeout.Infinite, CancellationToken));
            }
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}
