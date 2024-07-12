using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SearchService : ISearchService
    {
        private const string SearchProviderSettingName = "SearchProvider";

        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingRepository _settingRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IServiceProvider serviceProvider,
            ISettingRepository settingRepository,
            IPermissionRepository permissionRepository,
            ILogger<SearchService> logger)
        {
            _settingRepository = settingRepository;
            _permissionRepository = permissionRepository;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<SearchResults> GetSearchResultsAsync(SearchQuery searchQuery)
        {
            var searchProvider = GetSearchProvider(searchQuery.SiteId);
            var searchResults = await searchProvider.GetSearchResultsAsync(searchQuery);

            var totalResults = 0;

            // trim results based on permissions
            var results = searchResults.Where(i => IsVisible(i, searchQuery));

            if (searchQuery.SortDirection == SearchSortDirections.Descending)
            {
                switch (searchQuery.SortField)
                {
                    case SearchSortFields.Relevance:
                        results = results.OrderByDescending(i => i.Score).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    case SearchSortFields.Title:
                        results = results.OrderByDescending(i => i.Title).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    default:
                        results = results.OrderByDescending(i => i.ContentModifiedOn);
                        break;
                }
            }
            else
            {
                switch (searchQuery.SortField)
                {
                    case SearchSortFields.Relevance:
                        results = results.OrderBy(i => i.Score).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    case SearchSortFields.Title:
                        results = results.OrderBy(i => i.Title).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    default:
                        results = results.OrderBy(i => i.ContentModifiedOn);
                        break;
                }
            }

            // remove duplicated results based on page id for Page and Module types
            results = results.DistinctBy(i =>
            {
                if (i.EntityName == EntityNames.Page || i.EntityName == EntityNames.Module)
                {
                    var pageId = i.SearchContentProperties.FirstOrDefault(p => p.Name == Constants.SearchPageIdPropertyName)?.Value ?? string.Empty;
                    return !string.IsNullOrEmpty(pageId) ? pageId : i.UniqueKey;
                }
                else
                {
                    return i.UniqueKey;
                }
            });

            totalResults = results.Count();

            return new SearchResults
            {
                Results = results.Skip(searchQuery.PageIndex * searchQuery.PageSize).Take(searchQuery.PageSize).ToList(),
                TotalResults = totalResults
            };
        }

        private bool IsVisible(SearchContent searchContent, SearchQuery searchQuery)
        {
            var visible = true;
            foreach (var permission in searchContent.Permissions.Split(','))
            {
                var entityName = permission.Split(":")[0];
                var entityId = int.Parse(permission.Split(":")[1]);
                if (!HasViewPermission(searchQuery.SiteId, searchQuery.User, entityName, entityId))
                {
                    visible = false;
                    break;
                }
            }
            return visible;
        }

        private bool HasViewPermission(int siteId, User user, string entityName, int entityId)
        {
            var permissions = _permissionRepository.GetPermissions(siteId, entityName, entityId).ToList();
            return UserSecurity.IsAuthorized(user, PermissionNames.View, permissions);
        }

        public async Task<string> SaveSearchContentsAsync(List<SearchContent> searchContents, Dictionary<string, string> siteSettings)
        {
            var result = "";

            if (searchContents.Any())
            {
                var searchProvider = GetSearchProvider(searchContents.First().SiteId);

                foreach (var searchContent in searchContents)
                {
                    try
                    {
                        await searchProvider.SaveSearchContent(searchContent, siteSettings);
                    }
                    catch (Exception ex)
                    {
                        result += $"Error Saving Search Content With UniqueKey {searchContent.UniqueKey} - {ex.Message}<br />";
                    }
                }
            }

            return result;
        }

        private ISearchProvider GetSearchProvider(int siteId)
        {
            var providerName = GetSearchProviderSetting(siteId);
            var searchProviders = _serviceProvider.GetServices<ISearchProvider>();
            var provider = searchProviders.FirstOrDefault(i => i.Name == providerName);
            if (provider == null)
            {
                provider = searchProviders.FirstOrDefault(i => i.Name == Constants.DefaultSearchProviderName);
            }

            return provider;
        }

        private string GetSearchProviderSetting(int siteId)
        {
            var setting = _settingRepository.GetSetting(EntityNames.Site, siteId, SearchProviderSettingName);
            if (!string.IsNullOrEmpty(setting?.SettingValue))
            {
                return setting.SettingValue;
            }

            return Constants.DefaultSearchProviderName;
        }
    }
}
