using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Oqtane.Documentation;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Pages
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class PagesManager : ISearchable
    {
        private readonly IPageRepository _pageRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly ILogger<PagesManager> _logger;

        public PagesManager(IPageRepository pageRepository, ISettingRepository settingRepository, ILogger<PagesManager> logger)
        {
            _pageRepository = pageRepository;
            _settingRepository = settingRepository;
            _logger = logger;
        }

        public List<SearchContent> GetSearchContents(PageModule pageModule, DateTime startTime)
        {
            var pages = _pageRepository.GetPages(pageModule.Module.SiteId).Where(i => i.ModifiedOn >= startTime);
            var searchContentList = new List<SearchContent>();

            foreach (var page in pages)
            {
                try
                {
                    if (SearchUtils.IsSystemPage(page))
                    {
                        continue;
                    }

                    var searchContent = new SearchContent
                    {
                        UniqueKey = $"{EntityNames.Page}:{page.PageId}",
                        EntityName = EntityNames.Page,
                        EntityId = page.PageId,
                        SiteId = page.SiteId,
                        ContentAuthoredBy = page.ModifiedBy,
                        ContentAuthoredOn = page.ModifiedOn,
                        AdditionalContent = string.Empty,
                        Url = $"{(!string.IsNullOrEmpty(page.Path) && !page.Path.StartsWith("/") ? "/" : "")}{page.Path}",
                        Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name,
                        Description = string.Empty,
                        Body = $"{page.Name} {page.Title}",
                        IsActive = !page.IsDeleted && Utilities.IsPageModuleVisible(page.EffectiveDate, page.ExpiryDate) && AllowIndex(page)
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Search: Index page {page.PageId} failed.");
                }
            }

            return searchContentList;
        }

        private bool AllowIndex(Page page)
        {
            var setting = _settingRepository.GetSetting(EntityNames.Page, page.PageId, "AllowIndex");
            return setting == null || !bool.TryParse(setting.SettingValue, out bool allowed) || allowed;
        }
    }
}
