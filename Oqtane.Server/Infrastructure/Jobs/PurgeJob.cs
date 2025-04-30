using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class PurgeJob : HostedServiceBase
    {
        // JobType = "Oqtane.Infrastructure.PurgeJob, Oqtane.Server"

        public PurgeJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Purge Job";
            Frequency = "d"; // daily
            Interval = 1;
            StartDate = DateTime.ParseExact("03:00", "H:mm", null, System.Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime(); // 3 AM
            IsEnabled = true;
        }

        // job is executed for each tenant in installation
        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            // get services
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();
            var logRepository = provider.GetRequiredService<ILogRepository>();
            var visitorRepository = provider.GetRequiredService<IVisitorRepository>();
            var notificationRepository = provider.GetRequiredService<INotificationRepository>();
            var urlMappingRepository = provider.GetRequiredService<IUrlMappingRepository>();
            var installationManager = provider.GetRequiredService<IInstallationManager>();

            // iterate through sites for current tenant
            List<Site> sites = siteRepository.GetSites().ToList();
            foreach (Site site in sites)
            {
                log += "<br />Processing Site: " + site.Name + "<br />";
                int count;

                // get site settings
                var settings = settingRepository.GetSettings(EntityNames.Site, site.SiteId, EntityNames.Host, -1);

                // purge event log
                var retention = int.Parse(settingRepository.GetSettingValue(settings, "LogRetention", "30")); // 30 day default
                try
                {
                    count = logRepository.DeleteLogs(site.SiteId, retention);
                    log += count.ToString() + " Events Purged<br />";
                }
                catch (Exception ex)
                {
                    log += $"Error Purging Events - {ex.Message}<br />";
                }

                // purge visitors
                if (site.VisitorTracking)
                {
                    retention = int.Parse(settingRepository.GetSettingValue(settings, "VisitorRetention", "30")); // 30 day default
                    try
                    {
                        count = visitorRepository.DeleteVisitors(site.SiteId, retention);
                        log += count.ToString() + " Visitors Purged<br />";
                    }
                    catch (Exception ex)
                    {
                        log += $"Error Purging Visitors - {ex.Message}<br />";
                    }
                }

                // purge notifications
                retention = int.Parse(settingRepository.GetSettingValue(settings, "NotificationRetention", "30")); // 30 day default
                try
                {
                    count = notificationRepository.DeleteNotifications(site.SiteId, retention);
                    log += count.ToString() + " Notifications Purged<br />";
                }
                catch (Exception ex)
                {
                    log += $"Error Purging Notifications - {ex.Message}<br />";
                }

                // purge broken urls 
                retention = int.Parse(settingRepository.GetSettingValue(settings, "UrlMappingRetention", "30")); // 30 day default
                try
                {
                    count = urlMappingRepository.DeleteUrlMappings(site.SiteId, retention);
                    log += count.ToString() + " Broken Urls Purged<br />";
                }
                catch (Exception ex)
                {
                    log += $"Error Purging Broken Urls - {ex.Message}<br />";
                }
            }

            // register assemblies
            try
            {
                var assemblies = installationManager.RegisterAssemblies();
                log += "<br />" + assemblies.ToString() + " Assemblies Registered<br />";
            }
            catch (Exception ex)
            {
                log += $"<br />Error Registering Assemblies - {ex.Message}<br />";
            }

            return log;
        }
    }
}
