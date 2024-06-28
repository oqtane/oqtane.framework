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
                var page = _pageRepository.GetPage(pageModule.PageId);
                if(page == null || SearchUtils.IsSystemPage(page))
                {
                    continue;
                }

                var module = pageModule.Module;
                if (module.ModuleDefinition != null && module.ModuleDefinition.ServerManagerType != "")
                {
                    _logger.LogDebug($"Search: Begin index module {module.ModuleId}.");
                    var type = Type.GetType(module.ModuleDefinition.ServerManagerType);
                    if (type?.GetInterface(nameof(ISearchable)) != null)
                    {
                        try
                        {
                            var moduleSearch = (ISearchable)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                            var contentList = moduleSearch.GetSearchContents(module, startTime.GetValueOrDefault(DateTime.MinValue));
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
                            _logger.LogError(ex, $"Search: Index module {module.ModuleId} failed.");
                            handleError($"Search: Index module {module.ModuleId} failed: {ex.Message}");
                        }
                    }
                    _logger.LogDebug($"Search: End index module {module.ModuleId}.");
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


            if (searchContent.ContentModifiedOn == DateTime.MinValue)
            {
                searchContent.ContentModifiedOn = pageModule.ModifiedOn;
            }

            if (string.IsNullOrEmpty(searchContent.AdditionalContent))
            {
                searchContent.AdditionalContent = string.Empty;
            }

            var page = _pageRepository.GetPage(pageModule.PageId);

            if (string.IsNullOrEmpty(searchContent.Url) && page != null)
            {
                searchContent.Url = $"{(!string.IsNullOrEmpty(page.Path) && !page.Path.StartsWith("/") ? "/" : "")}{page.Path}";
            }

            if (string.IsNullOrEmpty(searchContent.Title) && page != null)
            {
                searchContent.Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name;
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
