using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public abstract Task<int> IndexContent(int siteId, DateTime? startDate, Func<List<SearchContent>, Task> processSearchContent, Func<string, Task> handleError);

        public virtual bool IsIndexEnabled(int siteId)
        {
            var settingName = string.Format(SearchIndexManagerEnabledSettingFormat, Name);
            var settingRepository = _serviceProvider.GetRequiredService<ISettingRepository>();
            var setting = settingRepository.GetSetting(EntityNames.Site, siteId, settingName);
            return setting == null || setting.SettingValue == "true";
        }
    }
}
