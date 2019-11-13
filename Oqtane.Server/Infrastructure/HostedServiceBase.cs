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

        // abstract method must be overridden by job
        public abstract void ExecuteJob(IServiceProvider provider);


        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // allows consumption of scoped services 
                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        string JobType = Utilities.GetFullTypeName(this.GetType().AssemblyQualifiedName);
                        IScheduleRepository ScheduleRepository = scope.ServiceProvider.GetRequiredService<IScheduleRepository>();
                        List<Schedule> schedules = ScheduleRepository.GetSchedules().ToList();
                        Schedule schedule = schedules.Where(item => item.JobType == JobType).FirstOrDefault();
                        if (schedule != null && schedule.IsActive)
                        {
                            ExecuteJob(scope.ServiceProvider);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch
            {
                // can occur during the initial installation as there is no DBContext
            }

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
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
