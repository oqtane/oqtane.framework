using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class SiteTaskJob : HostedServiceBase
    {
        public SiteTaskJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Site Task Job";
            Frequency = "m"; // run every minute
            Interval = 1;
            IsEnabled = true;
        }

        // job is executed for each tenant in installation
        public override async Task<string> ExecuteJobAsync(IServiceProvider provider)
        {
            var log = "";

            var tenantManager = provider.GetRequiredService<ITenantManager>();
            var tenant = tenantManager.GetTenant();

            // iterate through sites for current tenant
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var sites = siteRepository.GetSites().ToList();
            foreach (var site in sites.Where(item => !item.IsDeleted))
            {
                log += $"Processing Site: {site.Name}<br />";

                // get incomplete tasks for site
                var siteTaskRepository = provider.GetRequiredService<ISiteTaskRepository>();
                var tasks = siteTaskRepository.GetSiteTasks(site.SiteId).ToList();
                if (tasks != null && tasks.Any())
                {
                    foreach (var task in tasks)
                    {
                        log += $"Executing Task: {task.Name}<br />";

                        Type taskType = Type.GetType(task.Type);
                        if (taskType != null && taskType.GetInterface(nameof(ISiteTask)) != null)
                        {
                            try
                            {
                                tenantManager.SetAlias(tenant.TenantId, site.SiteId);

                                var taskObject = ActivatorUtilities.CreateInstance(provider, taskType);
                                var taskLog = ((ISiteTask)taskObject).ExecuteTask(provider, site, task.Parameters);
                                taskLog += await ((ISiteTask)taskObject).ExecuteTaskAsync(provider, site, task.Parameters);

                                task.Status = taskLog;
                            }
                            catch (Exception ex)
                            {
                                task.Status = "Error: " + ex.Message;
                            }
                        }
                        else
                        {
                            task.Status = $"Error: Task {task.Name} Has An Invalid Type {task.Type}<br />";
                        }

                        // update task
                        task.IsCompleted = true;
                        siteTaskRepository.UpdateSiteTask(task);

                        log += task.Status + "<br />";
                    }
                }
                else
                {
                    log += "No Tasks To Execute<br />";
                }
            }

            return log;
        }
    }
}
