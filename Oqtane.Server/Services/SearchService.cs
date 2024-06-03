using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SearchService : ISearchService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantManager _tenantManager;
        private readonly IAliasRepository _aliasRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly ILogger<SearchService> _logger;
        private readonly IMemoryCache _cache;

        public SearchService(
            IServiceProvider serviceProvider,
            ITenantManager tenantManager,
            IAliasRepository aliasRepository,
            ISettingRepository settingRepository,
            ILogger<SearchService> logger,
            IMemoryCache cache)
        {
            _tenantManager = tenantManager;
            _aliasRepository = aliasRepository;
            _settingRepository = settingRepository;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = cache;
        }

        public void IndexContent(int siteId, DateTime? startTime, Action<string> logNote, Action<string> handleError)
        {
            var searchEnabled = SearchEnabled(siteId);
            if(!searchEnabled)
            {
                logNote($"Search: Search is disabled on site {siteId}.<br />");
                return;
            }

            _logger.LogDebug($"Search: Start Index Content of {siteId}, Start Time: {startTime.GetValueOrDefault(DateTime.MinValue)}");

            var searchProvider = GetSearchProvider(siteId);

            SetTenant(siteId);

            if (startTime == null)
            {
                searchProvider.ResetIndex();
            }

            var searchIndexManagers = GetSearchIndexManagers(m => { });
            foreach (var searchIndexManager in searchIndexManagers)
            {
                if (!searchIndexManager.IsIndexEnabled(siteId))
                {
                    logNote($"Search: Ignore indexer {searchIndexManager.Name} because it's disabled.<br />");
                }
                else
                {
                    _logger.LogDebug($"Search: Begin Index {searchIndexManager.Name}");

                    var count = searchIndexManager.IndexDocuments(siteId, startTime, SaveIndexDocuments, handleError);
                    logNote($"Search: Indexer {searchIndexManager.Name} processed {count} documents.<br />");

                    _logger.LogDebug($"Search: End Index {searchIndexManager.Name}");
                }
            }
        }

        public async Task<SearchResults> SearchAsync(SearchQuery searchQuery)
        {
            var searchProvider = GetSearchProvider(searchQuery.SiteId);
            var searchResults = await searchProvider.SearchAsync(searchQuery, HasViewPermission);

            //generate the document url if it's not set.
            foreach (var result in searchResults.Results)
            {
                if(string.IsNullOrEmpty(result.Url))
                {
                    result.Url = GetDocumentUrl(result, searchQuery);
                }
            }

            return searchResults;
        }

        private ISearchProvider GetSearchProvider(int siteId)
        {
            var providerName = GetSearchProviderSetting(siteId);
            var searchProviders = _serviceProvider.GetServices<ISearchProvider>();
            var provider = searchProviders.FirstOrDefault(i => i.Name == providerName);
            if(provider == null)
            {
                provider = searchProviders.FirstOrDefault(i => i.Name == Constants.DefaultSearchProviderName);
            }

            return provider;
        }

        private string GetSearchProviderSetting(int siteId)
        {
            var setting = _settingRepository.GetSetting(EntityNames.Site, siteId, Constants.SearchProviderSettingName);
            if(!string.IsNullOrEmpty(setting?.SettingValue))
            {
                return setting.SettingValue;
            }

            return Constants.DefaultSearchProviderName;
        }

        private bool SearchEnabled(int siteId)
        {
            var setting = _settingRepository.GetSetting(EntityNames.Site, siteId, Constants.SearchEnabledSettingName);
            if (!string.IsNullOrEmpty(setting?.SettingValue))
            {
                return bool.TryParse(setting.SettingValue, out bool enabled) && enabled;
            }

            return true;
        }

        private void SetTenant(int siteId)
        {
            var alias = _aliasRepository.GetAliases().OrderBy(i => i.SiteId).ThenByDescending(i => i.IsDefault).FirstOrDefault(i => i.SiteId == siteId);
            _tenantManager.SetAlias(alias);
        }

        private IList<ISearchIndexManager> GetSearchIndexManagers(Action<ISearchIndexManager> initManager)
        {
            var managers = new List<ISearchIndexManager>();
            var managerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISearchIndexManager).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in managerTypes)
            {
                var manager = (ISearchIndexManager)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                initManager(manager);
                managers.Add(manager);
            }

            return managers.OrderBy(i => i.Priority).ToList();
        }

        private IList<ISearchResultManager> GetSearchResultManagers()
        {
            var managers = new List<ISearchResultManager>();
            var managerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISearchResultManager).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var type in managerTypes)
            {
                var manager = (ISearchResultManager)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                managers.Add(manager);
            }

            return managers.ToList();
        }

        private void SaveIndexDocuments(IList<SearchDocument> searchDocuments)
        {
            if(searchDocuments.Any())
            {
                var searchProvider = GetSearchProvider(searchDocuments.First().SiteId);

                foreach (var searchDocument in searchDocuments)
                {
                    try
                    {
                        searchProvider.SaveDocument(searchDocument);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, $"Search: Save search document {searchDocument.UniqueKey} failed.");
                    }
                }

                //commit the index changes
                searchProvider.Commit();
            }
        }

        private bool HasViewPermission(SearchDocument searchDocument, SearchQuery searchQuery)
        {
            var searchResultManager = GetSearchResultManagers().FirstOrDefault(i => i.Name == searchDocument.IndexerName);
            if (searchResultManager != null)
            {
                return searchResultManager.Visible(searchDocument, searchQuery);
            }
            return true;
        }

        private string GetDocumentUrl(SearchResult result, SearchQuery searchQuery)
        {
            var searchResultManager = GetSearchResultManagers().FirstOrDefault(i => i.Name == result.IndexerName);
            if(searchResultManager != null)
            {
                return searchResultManager.GetUrl(result, searchQuery);
            }

            return string.Empty;
        }
    }
}
