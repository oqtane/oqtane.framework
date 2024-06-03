using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Managers.Search
{
    public class PageSearchIndexManager : SearchIndexManagerBase
    {
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

        public override string Name => Constants.PageSearchIndexManagerName;

        public override int Priority => Constants.PageSearchIndexManagerPriority;

        public override int IndexDocuments(int siteId, DateTime? startTime, Action<IList<SearchDocument>> processSearchDocuments, Action<string> handleError)
        {
            var startTimeValue = startTime.GetValueOrDefault(DateTime.MinValue);
            var pages = _pageRepository.GetPages(siteId).Where(i => i.ModifiedOn >= startTimeValue);
            var searchDocuments = new List<SearchDocument>();

            foreach(var page in pages)
            {
                try
                {
                    if(IsSystemPage(page))
                    {
                        continue;
                    }

                    var document = new SearchDocument
                    {
                        EntryId = page.PageId,
                        IndexerName = Name,
                        SiteId = page.SiteId,
                        LanguageCode = string.Empty,
                        ModifiedTime = page.ModifiedOn,
                        AdditionalContent = string.Empty,
                        Url = page.Url ?? string.Empty,
                        Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name,
                        Description = string.Empty,
                        Body = $"{page.Name} {page.Title}"
                    };

                    if (document.Properties == null)
                    {
                        document.Properties = new List<SearchDocumentProperty>();
                    }

                    if (!document.Properties.Any(i => i.Name == Constants.SearchPageIdPropertyName))
                    {
                        document.Properties.Add(new SearchDocumentProperty { Name = Constants.SearchPageIdPropertyName, Value = page.PageId.ToString() });
                    }

                    searchDocuments.Add(document);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Search: Index page {page.PageId} failed.");
                    handleError($"Search: Index page {page.PageId} failed: {ex.Message}");
                }
            }

            processSearchDocuments(searchDocuments);

            return searchDocuments.Count;
        }

        private bool IsSystemPage(Models.Page page)
        {
            return page.Path.Contains("admin") || page.Path == "login" || page.Path == "register" || page.Path == "profile";
        }
    }
}
