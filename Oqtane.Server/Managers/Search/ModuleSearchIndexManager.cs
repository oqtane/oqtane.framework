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

        public override string Name => Constants.ModuleSearchIndexManagerName;

        public override int Priority => Constants.ModuleSearchIndexManagerPriority;

        public override int IndexDocuments(int siteId, DateTime? startTime, Action<IList<SearchDocument>> processSearchDocuments, Action<string> handleError)
        {
            var pageModules = _pageModuleRepostory.GetPageModules(siteId).DistinctBy(i => i.ModuleId);
            var searchDocuments = new List<SearchDocument>();

            foreach(var pageModule in pageModules)
            {
                var module = pageModule.Module;
                if (module.ModuleDefinition.ServerManagerType != "")
                {
                    _logger.LogDebug($"Search: Begin index module {module.ModuleId}.");
                    var type = Type.GetType(module.ModuleDefinition.ServerManagerType);
                    if (type?.GetInterface("IModuleSearch") != null)
                    {
                        try
                        {
                            var moduleSearch = (IModuleSearch)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                            var documents = moduleSearch.GetSearchDocuments(module, startTime.GetValueOrDefault(DateTime.MinValue));
                            if(documents != null)
                            {
                                foreach(var document in documents)
                                {
                                    SaveModuleMetaData(document, pageModule);

                                    searchDocuments.Add(document);
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

            processSearchDocuments(searchDocuments);

            return searchDocuments.Count;
        }

        private void SaveModuleMetaData(SearchDocument document, PageModule pageModule)
        {
            
            document.EntryId = pageModule.ModuleId;
            document.IndexerName = Name;
            document.SiteId = pageModule.Module.SiteId;
            document.LanguageCode = string.Empty;

            if(document.ModifiedTime == DateTime.MinValue)
            {
                document.ModifiedTime = pageModule.ModifiedOn;
            }

            if (string.IsNullOrEmpty(document.AdditionalContent))
            {
                document.AdditionalContent = string.Empty;
            }

            var page = _pageRepository.GetPage(pageModule.PageId);

            if (string.IsNullOrEmpty(document.Url) && page != null)
            {
                document.Url = page.Url;
            }

            if (string.IsNullOrEmpty(document.Title) && page != null)
            {
                document.Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name;
            }

            if (document.Properties == null)
            {
                document.Properties = new List<SearchDocumentProperty>();
            }

            if(!document.Properties.Any(i => i.Name == Constants.SearchPageIdPropertyName))
            {
                document.Properties.Add(new SearchDocumentProperty { Name = Constants.SearchPageIdPropertyName, Value = pageModule.PageId.ToString() });
            }

            if (!document.Properties.Any(i => i.Name == Constants.SearchModuleIdPropertyName))
            {
                document.Properties.Add(new SearchDocumentProperty { Name = Constants.SearchModuleIdPropertyName, Value = pageModule.ModuleId.ToString() });
            }
        }
    }
}
