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
            StartDate = DateTime.ParseExact("03:00", "H:mm", null, System.Globalization.DateTimeStyles.None); // 3 AM
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

            // iterate through sites for current tenant
            List<Site> sites = siteRepository.GetSites().ToList();
            foreach (Site site in sites)
            {
                log += "Processing Site: " + site.Name + "<br />";

                // get site settings
                Dictionary<string, string> settings = GetSettings(settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToList());

                // purge event log
                int retention = 30; // 30 days
                if (settings.ContainsKey("LogRetention") && !string.IsNullOrEmpty(settings["LogRetention"]))
                {
                    retention = int.Parse(settings["LogRetention"]);
                }
                int count = logRepository.DeleteLogs(retention);
                log += count.ToString() + " Events Purged<br />";

                // purge visitors
                if (site.VisitorTracking)
                {
                    retention = 30; // 30 days
                    if (settings.ContainsKey("VisitorRetention") && !string.IsNullOrEmpty(settings["VisitorRetention"]))
                    {
                        retention = int.Parse(settings["VisitorRetention"]);
                    }
                    count = visitorRepository.DeleteVisitors(retention);
                    log += count.ToString() + " Visitors Purged<br />";
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
