using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Managers.Search
{
    public abstract class SearchIndexManagerBase : ISearchIndexManager
    {
        private const string SearchIndexManagerEnabledSettingFormat = "SearchIndexManager_{0}_Enabled";

        private readonly IServiceProvider _serviceProvider;

        public SearchIndexManagerBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract int Priority { get; }

        public abstract string Name { get; }

        public abstract int IndexContent(int siteId, DateTime? startDate, Action<IList<SearchContent>> processSearchContent, Action<string> handleError);

        public virtual bool IsIndexEnabled(int siteId)
        {
            var settingName = string.Format(SearchIndexManagerEnabledSettingFormat, Name);
            var settingRepository = _serviceProvider.GetRequiredService<ISettingRepository>();
            var setting = settingRepository.GetSetting(EntityNames.Site, siteId, settingName);
            return setting == null || setting.SettingValue == "true";
        }
    }
}
