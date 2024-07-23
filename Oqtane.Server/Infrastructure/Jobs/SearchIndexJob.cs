using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class SearchIndexJob : HostedServiceBase
    {
        private const string SearchLastIndexedOnSetting = "Search_LastIndexedOn";
        private const string SearchEnabledSetting = "Search_Enabled";
        private const string SearchIgnorePagesSetting = "Search_IgnorePages";
        private const string SearchIgnoreEntitiesSetting = "Search_IgnoreEntities";

        public SearchIndexJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Search Index Job";
            Frequency = "m"; // run every minute.
            Interval = 1;
            IsEnabled = true;
        }

        public override async Task<string> ExecuteJobAsync(IServiceProvider provider)
        {
            string log = "";

            // get services
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            var pageRepository = provider.GetRequiredService<IPageRepository>();
            var pageModuleRepository = provider.GetRequiredService<IPageModuleRepository>();
            var searchService = provider.GetRequiredService<ISearchService>();

            var sites = siteRepository.GetSites().ToList();
            foreach (var site in sites)
            {
                log += $"Indexing Site: {site.Name}<br />";

                // initialize
                var siteSettings = settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);

                if (!Convert.ToBoolean(siteSettings.GetValue(SearchEnabledSetting, "true")))
                {
                    log += $"Indexing Disabled<br />";
                    continue;
                }

                var tenantId = tenantManager.GetTenant().TenantId;
                tenantManager.SetAlias(tenantId, site.SiteId);

                var currentTime = DateTime.UtcNow;
                var lastIndexedOn = Convert.ToDateTime(siteSettings.GetValue(SearchLastIndexedOnSetting, DateTime.MinValue.ToString()));

                var ignorePages = siteSettings.GetValue(SearchIgnorePagesSetting, "").Split(',');
                var ignoreEntities = siteSettings.GetValue(SearchIgnoreEntitiesSetting, "").Split(',');

                var pages = pageRepository.GetPages(site.SiteId);
                var pageModules = pageModuleRepository.GetPageModules(site.SiteId);
                var searchContents = new List<SearchContent>();

                // index pages
                foreach (var page in pages)
                {
                    if (!string.IsNullOrEmpty(page.Path) && (Constants.InternalPagePaths.Contains(page.Path) || ignorePages.Contains(page.Path)))
                    {
                        continue;
                    }

                    bool changed = false;
                    bool removed = false;

                    if (page.ModifiedOn >= lastIndexedOn && !ignoreEntities.Contains(EntityNames.Page))
                    {
                        changed = true;
                        removed = page.IsDeleted || !Utilities.IsEffectiveAndNotExpired(page.EffectiveDate, page.ExpiryDate);

                        var searchContent = new SearchContent
                        {
                            SiteId = page.SiteId,
                            EntityName = EntityNames.Page,
                            EntityId = page.PageId.ToString(),
                            Title = !string.IsNullOrEmpty(page.Title) ? page.Title : page.Name,
                            Description = string.Empty,
                            Body = string.Empty,
                            Url = $"{(!string.IsNullOrEmpty(page.Path) && !page.Path.StartsWith("/") ? "/" : "")}{page.Path}",
                            Permissions = $"{EntityNames.Page}:{page.PageId}",
                            ContentModifiedBy = page.ModifiedBy,
                            ContentModifiedOn = page.ModifiedOn,
                            AdditionalContent = string.Empty,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = removed,
                            TenantId = tenantId
                        };
                        searchContents.Add(searchContent);
                    }

                    // index modules
                    foreach (var pageModule in pageModules.Where(item => item.PageId == page.PageId))
                    {
                        if (pageModule.ModifiedOn >= lastIndexedOn && !changed)
                        {
                            changed = true;
                        }

                        var searchable = false;
                        if (pageModule.Module.ModuleDefinition != null && pageModule.Module.ModuleDefinition.ServerManagerType != "")
                        {
                            Type type = Type.GetType(pageModule.Module.ModuleDefinition.ServerManagerType);
                            if (type?.GetInterface(nameof(ISearchable)) != null)
                            {
                                try
                                {
                                    searchable = true;

                                    // determine if reindexing is necessary
                                    var lastindexedon = (changed) ? DateTime.MinValue : lastIndexedOn;

                                    // index module content
                                    var serverManager = (ISearchable)ActivatorUtilities.CreateInstance(provider, type);
                                    var searchcontents = await serverManager.GetSearchContentsAsync(pageModule, lastindexedon);
                                    if (searchcontents != null)
                                    {
                                        foreach (var searchContent in searchcontents)
                                        {
                                            if (!ignoreEntities.Contains(searchContent.EntityName))
                                            {
                                                ValidateSearchContent(searchContent, pageModule, tenantId, removed);
                                                searchContents.Add(searchContent);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log += $"Error Indexing Module {pageModule.Module.ModuleDefinition.Name} - {ex.Message}<br />";
                                }
                            }
                        }

                        if (!searchable && changed && !ignoreEntities.Contains(EntityNames.Module))
                        {
                            // module does not implement ISearchable
                            var searchContent = new SearchContent();
                            ValidateSearchContent(searchContent, pageModule, tenantId, removed);
                            searchContents.Add(searchContent);
                        }
                    }
                }

                // save search contents
                log += await searchService.SaveSearchContentsAsync(searchContents, siteSettings);
                log += $"Items Indexed: {searchContents.Count}<br />";

                // update last indexed on
                SaveSearchLastIndexedOn(settingRepository, site.SiteId, currentTime);
            }

            return log;
        }


        private void ValidateSearchContent(SearchContent searchContent, PageModule pageModule, int tenantId, bool removed)
        {
            // set default values
            searchContent.SiteId = pageModule.Module.SiteId;
            searchContent.TenantId = tenantId;
            searchContent.CreatedOn = DateTime.UtcNow;

            if (string.IsNullOrEmpty(searchContent.EntityName))
            {
                searchContent.EntityName = EntityNames.Module;
            }

            if (string.IsNullOrEmpty(searchContent.EntityId))
            {
                searchContent.EntityId = pageModule.ModuleId.ToString();
            }

            if (string.IsNullOrEmpty(searchContent.Title))
            {
                searchContent.Title = string.Empty;
                if (!string.IsNullOrEmpty(pageModule.Title))
                {
                    searchContent.Title = pageModule.Title;
                }
                else if (pageModule.Page != null)
                {
                    searchContent.Title = !string.IsNullOrEmpty(pageModule.Page.Title) ? pageModule.Page.Title : pageModule.Page.Name;
                }
            }

            if (searchContent.Description == null) { searchContent.Description = string.Empty;}
            if (searchContent.Body == null) { searchContent.Body = string.Empty; }

            if (string.IsNullOrEmpty(searchContent.Url))
            {
                searchContent.Url = string.Empty;
                if (pageModule.Page != null)
                {
                    searchContent.Url = $"{(!string.IsNullOrEmpty(pageModule.Page.Path) && !pageModule.Page.Path.StartsWith("/") ? "/" : "")}{pageModule.Page.Path}";
                }
            }

            if (string.IsNullOrEmpty(searchContent.Permissions))
            {
                searchContent.Permissions = $"{EntityNames.Module}:{pageModule.ModuleId},{EntityNames.Page}:{pageModule.PageId}";
            }

            if (string.IsNullOrEmpty(searchContent.ContentModifiedBy))
            {
                searchContent.ContentModifiedBy = pageModule.ModifiedBy;
            }

            if (searchContent.ContentModifiedOn == DateTime.MinValue)
            {
                searchContent.ContentModifiedOn = pageModule.ModifiedOn;
            }

            if (string.IsNullOrEmpty(searchContent.AdditionalContent))
            {
                searchContent.AdditionalContent = string.Empty;
            }

            if (removed || pageModule.IsDeleted || !Utilities.IsEffectiveAndNotExpired(pageModule.EffectiveDate, pageModule.ExpiryDate))
            {
                searchContent.IsDeleted = true;
            }
        }

        private void SaveSearchLastIndexedOn(ISettingRepository settingRepository, int siteId, DateTime lastIndexedOn)
        {
            var setting = settingRepository.GetSetting(EntityNames.Site, siteId, SearchLastIndexedOnSetting);
            if (setting == null)
            {
                setting = new Setting
                {
                    EntityName = EntityNames.Site,
                    EntityId = siteId,
                    SettingName = SearchLastIndexedOnSetting,
                    SettingValue = Convert.ToString(lastIndexedOn),
                };
                settingRepository.AddSetting(setting);
            }
            else
            {
                setting.SettingValue = Convert.ToString(lastIndexedOn);
                settingRepository.UpdateSetting(setting);
            }
        }
    }
}
