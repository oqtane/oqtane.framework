using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SearchService : ISearchService
    {
        private const string SearchProviderSettingName = "Search_SearchProvider";

        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingRepository _settingRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly IHttpContextAccessor _accessor;

        public SearchService(
            IServiceProvider serviceProvider,
            ISettingRepository settingRepository,
            IUserPermissions userPermissions,
            IHttpContextAccessor accessor)
        {
            _settingRepository = settingRepository;
            _serviceProvider = serviceProvider;
            _userPermissions = userPermissions;
            _accessor = accessor;
        }

        public async Task<SearchResults> GetSearchResultsAsync(SearchQuery searchQuery)
        {
            var searchProvider = GetSearchProvider(searchQuery.SiteId);
            var searchResults = await searchProvider.GetSearchResultsAsync(searchQuery);

            var totalResults = 0;

            // trim results
            var results = searchResults.Where(i => HasViewPermission(i, searchQuery))
                .OrderBy(i => i.Url).ThenByDescending(i => i.Score)
                .DistinctBy(i => i.Url);

            // sort results
            if (searchQuery.SortOrder == SearchSortOrder.Descending)
            {
                switch (searchQuery.SortField)
                {
                    case SearchSortField.Relevance:
                        results = results.OrderByDescending(i => i.Score).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    case SearchSortField.Title:
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
                    case SearchSortField.Relevance:
                        results = results.OrderBy(i => i.Score).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    case SearchSortField.Title:
                        results = results.OrderBy(i => i.Title).ThenByDescending(i => i.ContentModifiedOn);
                        break;
                    default:
                        results = results.OrderBy(i => i.ContentModifiedOn);
                        break;
                }
            }

            totalResults = results.Count();

            return new SearchResults
            {
                Results = results.Skip(searchQuery.PageIndex * searchQuery.PageSize).Take(searchQuery.PageSize).ToList(),
                TotalResults = totalResults
            };
        }

        private bool HasViewPermission(SearchContent searchContent, SearchQuery searchQuery)
        {
            var visible = true;
            foreach (var permission in searchContent.Permissions.Split(','))
            {
                if (permission.Contains(":")) // permission
                {
                    var entityName = permission.Split(":")[0];
                    var entityId = int.Parse(permission.Split(":")[1]);
                    if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, searchQuery.SiteId, entityName, entityId, PermissionNames.View))
                    {
                        visible = false;
                        break;
                    }
                }
                else // role name
                {
                    if (!_accessor.HttpContext.User.IsInRole(permission))
                    {
                        visible = false;
                        break;
                    }
                }
            }
            return visible;
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
