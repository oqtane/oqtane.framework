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

            // iterate through sites for current tenant
            List<Site> sites = siteRepository.GetSites().ToList();
            foreach (Site site in sites)
            {
                log += "Processing Site: " + site.Name + "<br />";
                int retention;
                int count;

                // get site settings
                Dictionary<string, string> settings = GetSettings(settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToList());

                // purge event log
                retention = 30; // 30 days
                if (settings.ContainsKey("LogRetention") && !string.IsNullOrEmpty(settings["LogRetention"]))
                {
                    retention = int.Parse(settings["LogRetention"]);
                }
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
                    retention = 30; // 30 days
                    if (settings.ContainsKey("VisitorRetention") && !string.IsNullOrEmpty(settings["VisitorRetention"]))
                    {
                        retention = int.Parse(settings["VisitorRetention"]);
                    }
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
                retention = 30; // 30 days
                if (settings.ContainsKey("NotificationRetention") && !string.IsNullOrEmpty(settings["NotificationRetention"]))
                {
                    retention = int.Parse(settings["NotificationRetention"]);
                }
                try
                {
                    count = notificationRepository.DeleteNotifications(site.SiteId, retention);
                    log += count.ToString() + " Notifications Purged<br />";
                }
                catch (Exception ex)
                {
                    log += $"Error Purging Notifications - {ex.Message}<br />";
                }
            }

            return log;
        }

        private Dictionary<string, string> GetSettings(List<Setting> settings)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (Setting setting in settings.OrderBy(item => item.SettingName).ToList())
            {
                dictionary.Add(setting.SettingName, setting.SettingValue);
            }
            return dictionary;
        }
    }
}
