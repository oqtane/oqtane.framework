using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Managers.Search
{
    public class PageSearchIndexManager : SearchIndexManagerBase
    {
        private const int PageSearchIndexManagerPriority = 100;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleSearchIndexManager> _logger;
        private readonly IPageRepository _pageRepository;

        public PageSearchIndexManager(
            IServiceProvider serviceProvider,
            ILogger<ModuleSearchIndexManager> logger,
            IPageRepository pageRepository)
            : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _pageRepository = pageRepository;
        }

        public override string Name => EntityNames.Page;

        public override int Priority => PageSearchIndexManagerPriority;

        public override int IndexContent(int siteId, DateTime? startTime, Action<List<SearchContent>> processSearchContent, Action<string> handleError)
        {
            var startTimeValue = startTime.GetValueOrDefault(DateTime.MinValue);
            var pages = _pageRepository.GetPages(siteId).Where(i => i.ModifiedOn >= startTimeValue);
            var searchContentList = new List<SearchContent>();

            foreach(var page in pages)
            {
                try
                {
                    if(SearchUtils.IsSystemPage(page))
                    {
                        continue;
                    }

                    var searchContent = new SearchContent
                    {
                        SiteId = page.SiteId,
                        EntityName = EntityNames.Page,
                        EntityId = page.PageId.ToString(),
                        Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name,
                        Description = string.Empty,
                        Body = $"{page.Name} {page.Title}",
                        Url = $"{(!string.IsNullOrEmpty(page.Path) && !page.Path.StartsWith("/") ? "/" : "")}{page.Path}",
                        Permissions = $"{EntityNames.Page}:{page.PageId}",
                        ContentModifiedBy = page.ModifiedBy,
                        ContentModifiedOn = page.ModifiedOn,
                        AdditionalContent = string.Empty,
                        CreatedOn = DateTime.UtcNow
                    };

                    if (searchContent.SearchContentProperties == null)
                    {
                        searchContent.SearchContentProperties = new List<SearchContentProperty>();
                    }

                    if (!searchContent.SearchContentProperties.Any(i => i.Name == Constants.SearchPageIdPropertyName))
                    {
                        searchContent.SearchContentProperties.Add(new SearchContentProperty { Name = Constants.SearchPageIdPropertyName, Value = page.PageId.ToString() });
                    }

                    searchContentList.Add(searchContent);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Search: Index page {page.PageId} failed.");
                    handleError($"Search: Index page {page.PageId} failed: {ex.Message}");
                }
            }

            processSearchContent(searchContentList);

            return searchContentList.Count;
        }
    }
}
