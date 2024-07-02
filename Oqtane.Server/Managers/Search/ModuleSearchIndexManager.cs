using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Managers.Search
{
    public class ModuleSearchIndexManager : SearchIndexManagerBase
    {
        public const int ModuleSearchIndexManagerPriority = 200;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleSearchIndexManager> _logger;
        private readonly IPageModuleRepository _pageModuleRepostory;
        private readonly IPageRepository _pageRepository;

        public ModuleSearchIndexManager(
            IServiceProvider serviceProvider,
            IPageModuleRepository pageModuleRepostory,
            ILogger<ModuleSearchIndexManager> logger,
            IPageRepository pageRepository)
            : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _pageModuleRepostory = pageModuleRepostory;
            _pageRepository = pageRepository;
        }

        public override string Name => EntityNames.Module;

        public override int Priority => ModuleSearchIndexManagerPriority;

        public override int IndexContent(int siteId, DateTime? startTime, Action<List<SearchContent>> processSearchContent, Action<string> handleError)
        {
            var pageModules = _pageModuleRepostory.GetPageModules(siteId).DistinctBy(i => i.ModuleId);
            var searchContentList = new List<SearchContent>();

            foreach(var pageModule in pageModules)
            {
                if(pageModule.Page == null || SearchUtils.IsSystemPage(pageModule.Page))
                {
                    continue;
                }

                if (pageModule.Module.ModuleDefinition != null && pageModule.Module.ModuleDefinition.ServerManagerType != "")
                {
                    _logger.LogDebug($"Search: Begin index module {pageModule.ModuleId}.");
                    var type = Type.GetType(pageModule.Module.ModuleDefinition.ServerManagerType);
                    if (type?.GetInterface(nameof(ISearchable)) != null)
                    {
                        try
                        {
                            var moduleSearch = (ISearchable)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                            var contentList = moduleSearch.GetSearchContents(pageModule, startTime.GetValueOrDefault(DateTime.MinValue));
                            if(contentList != null)
                            {
                                foreach(var searchContent in contentList)
                                {
                                    SaveModuleMetaData(searchContent, pageModule);

                                    searchContentList.Add(searchContent);
                                }
                            }
                            
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, $"Search: Index module {pageModule.ModuleId} failed.");
                            handleError($"Search: Index module {pageModule.ModuleId} failed: {ex.Message}");
                        }
                    }
                    _logger.LogDebug($"Search: End index module {pageModule.ModuleId}.");
                }
            }

            processSearchContent(searchContentList);

            return searchContentList.Count;
        }

        private void SaveModuleMetaData(SearchContent searchContent, PageModule pageModule)
        {
            searchContent.SiteId = pageModule.Module.SiteId;

            if(string.IsNullOrEmpty(searchContent.EntityName))
            {
                searchContent.EntityName = EntityNames.Module;
            }

            if(string.IsNullOrEmpty(searchContent.EntityId))
            {
                searchContent.EntityId = pageModule.ModuleId.ToString();
            }

            if (string.IsNullOrEmpty(searchContent.Permissions))
            {
                searchContent.Permissions = $"{EntityNames.Module}:{pageModule.ModuleId},{EntityNames.Page}:{pageModule.PageId}";
            }

            if (searchContent.ContentModifiedOn == DateTime.MinValue)
            {
                searchContent.ContentModifiedOn = pageModule.ModifiedOn;
            }

            if (string.IsNullOrEmpty(searchContent.AdditionalContent))
            {
                searchContent.AdditionalContent = string.Empty;
            }

            if (pageModule.Page != null)
            {
                if (string.IsNullOrEmpty(searchContent.Url))
                {
                    searchContent.Url = $"{(!string.IsNullOrEmpty(pageModule.Page.Path) && !pageModule.Page.Path.StartsWith("/") ? "/" : "")}{pageModule.Page.Path}";
                }

                if (string.IsNullOrEmpty(searchContent.Title))
                {
                    searchContent.Title = !string.IsNullOrEmpty(pageModule.Page.Title) ? pageModule.Page.Title : pageModule.Page.Name;
                }
            }

            if (searchContent.SearchContentProperties == null)
            {
                searchContent.SearchContentProperties = new List<SearchContentProperty>();
            }

            if(!searchContent.SearchContentProperties.Any(i => i.Name == Constants.SearchPageIdPropertyName))
            {
                searchContent.SearchContentProperties.Add(new SearchContentProperty { Name = Constants.SearchPageIdPropertyName, Value = pageModule.PageId.ToString() });
            }

            if (!searchContent.SearchContentProperties.Any(i => i.Name == Constants.SearchModuleIdPropertyName))
            {
                searchContent.SearchContentProperties.Add(new SearchContentProperty { Name = Constants.SearchModuleIdPropertyName, Value = pageModule.ModuleId.ToString() });
            }
        }
    }
}
