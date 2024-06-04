using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class SearchIndexJob : HostedServiceBase
    {
        private const string SearchIndexStartTimeSettingName = "SearchIndex_StartTime";

        public SearchIndexJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Search Index Job";
            Frequency = "m"; // run every minute.
            Interval = 1;
            IsEnabled = true;
        }

        public override string ExecuteJob(IServiceProvider provider)
        {
            // get services
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();
            var logRepository = provider.GetRequiredService<ILogRepository>();
            var searchService = provider.GetRequiredService<ISearchService>();

            var sites = siteRepository.GetSites().ToList();
            var logs = new StringBuilder();

            foreach (var site in sites)
            {
                var startTime = GetSearchStartTime(site.SiteId, settingRepository);
                logs.AppendLine($"Search: Begin index site: {site.Name}<br />");
                var currentTime = DateTime.UtcNow;

                searchService.IndexContent(site.SiteId, startTime, logNote =>
                {
                    logs.AppendLine(logNote);
                }, handleError =>
                {
                    logs.AppendLine(handleError);
                });

                UpdateSearchStartTime(site.SiteId, currentTime, settingRepository);

                logs.AppendLine($"Search: End index site: {site.Name}<br />");
            }

            return logs.ToString();
        }

        private DateTime? GetSearchStartTime(int siteId, ISettingRepository settingRepository)
        {
            var setting = settingRepository.GetSetting(EntityNames.Site, siteId, SearchIndexStartTimeSettingName);
            if(setting == null)
            {
                return null;
            }

            return Convert.ToDateTime(setting.SettingValue);
        }

        private void UpdateSearchStartTime(int siteId, DateTime startTime, ISettingRepository settingRepository)
        {
            var setting = settingRepository.GetSetting(EntityNames.Site, siteId, SearchIndexStartTimeSettingName);
            if (setting == null)
            {
                setting = new Setting
                {
                    EntityName = EntityNames.Site,
                    EntityId = siteId,
                    SettingName = SearchIndexStartTimeSettingName,
                    SettingValue = Convert.ToString(startTime),
                };

                settingRepository.AddSetting(setting);
            }
            else
            {
                setting.SettingValue = Convert.ToString(startTime);
                settingRepository.UpdateSetting(setting);
            }
        }
    }
}
