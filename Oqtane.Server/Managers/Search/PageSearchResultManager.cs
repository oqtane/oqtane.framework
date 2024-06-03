using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Managers.Search
{
    public class PageSearchResultManager : ISearchResultManager
    {
        public string Name => Constants.PageSearchIndexManagerName;

        private readonly IServiceProvider _serviceProvider;

        public PageSearchResultManager(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string GetUrl(SearchResult searchResult, SearchQuery searchQuery)
        {
            var pageRepository = _serviceProvider.GetRequiredService<IPageRepository>();
            var page = pageRepository.GetPage(searchResult.EntryId);
            if (page != null)
            {
                return $"{searchQuery.Alias.Protocol}{searchQuery.Alias.Name}{(!string.IsNullOrEmpty(page.Path) && !page.Path.StartsWith("/") ? "/" : "")}{page.Path}";
            }

            return string.Empty;
        }

        public bool Visible(SearchDocument searchResult, SearchQuery searchQuery)
        {
            var pageRepository = _serviceProvider.GetRequiredService<IPageRepository>();
            var page = pageRepository.GetPage(searchResult.EntryId);

            return page != null && !page.IsDeleted
                    && UserSecurity.IsAuthorized(searchQuery.User, PermissionNames.View, page.PermissionList)
                    && (Utilities.IsPageModuleVisible(page.EffectiveDate, page.ExpiryDate) || UserSecurity.IsAuthorized(searchQuery.User, PermissionNames.Edit, page.PermissionList));
        }
    }
}
