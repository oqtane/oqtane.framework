using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class GlobalReplaceJob : HostedServiceBase
    {
        public GlobalReplaceJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Global Replace Job";
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

            var sites = siteRepository.GetSites().ToList();
            foreach (var site in sites.Where(item => !item.IsDeleted))
            {
                log += $"Processing Site: {site.Name}<br />";

                // get global replace items in order by date/time submitted
                var globalReplaceSettings = settingRepository.GetSettings(EntityNames.Site, site.SiteId)
                    .Where(item => item.SettingName.StartsWith("GlobalReplace_"))
                    .OrderBy(item => item.SettingName);

                if (globalReplaceSettings != null && globalReplaceSettings.Any())
                {
                    // get first item
                    var globalReplace = JsonSerializer.Deserialize<GlobalReplace>(globalReplaceSettings.First().SettingValue);

                    var find = globalReplace.Find;
                    var replace = globalReplace.Replace;
                    var comparisonType = (globalReplace.CaseSensitive) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                    log += $"Replacing: '{find}' With: '{replace}' Case Sensitive: {globalReplace.CaseSensitive}<br />";

                    var tenantId = tenantManager.GetTenant().TenantId;
                    tenantManager.SetAlias(tenantId, site.SiteId);

                    var changed = false;
                    if (site.Name != null && site.Name.Contains(find, comparisonType))
                    {
                        site.Name = site.Name.Replace(find, replace, comparisonType);
                        changed = true;
                    }
                    if (site.HeadContent != null && site.HeadContent.Contains(find, comparisonType))
                    {
                        site.HeadContent = site.HeadContent.Replace(find, replace, comparisonType);
                        changed = true;
                    }
                    if (site.BodyContent != null && site.BodyContent.Contains(find, comparisonType))
                    {
                        site.BodyContent = site.BodyContent.Replace(find, replace, comparisonType);
                        changed = true;
                    }
                    if (changed && globalReplace.Site)
                    {
                        siteRepository.UpdateSite(site);
                        log += $"Site Updated<br />";
                    }

                    var pages = pageRepository.GetPages(site.SiteId);
                    var pageModules = pageModuleRepository.GetPageModules(site.SiteId);

                    // iterate pages
                    foreach (var page in pages)
                    {
                        // page properties
                        changed = false;
                        if (page.Name != null && page.Name.Contains(find, comparisonType))
                        {
                            page.Name = page.Name.Replace(find, replace, comparisonType);
                            changed = true;
                        }
                        if (page.Title != null && page.Title.Contains(find, comparisonType))
                        {
                            page.Title = page.Title.Replace(find, replace, comparisonType);
                            changed = true;
                        }
                        if (page.HeadContent != null && page.HeadContent.Contains(find, comparisonType))
                        {
                            page.HeadContent = page.HeadContent.Replace(find, replace, comparisonType);
                            changed = true;
                        }
                        if (page.BodyContent != null && page.BodyContent.Contains(find, comparisonType))
                        {
                            page.BodyContent = page.BodyContent.Replace(find, replace, comparisonType);
                            changed = true;
                        }
                        if (changed && globalReplace.Pages)
                        {
                            pageRepository.UpdatePage(page);
                            log += $"Page Updated: /{page.Path}<br />";
                        }

                        foreach (var pageModule in pageModules.Where(item => item.PageId == page.PageId))
                        {
                            // pagemodule properties
                            changed = false;
                            if (pageModule.Title != null && pageModule.Title.Contains(find, comparisonType))
                            {
                                pageModule.Title = pageModule.Title.Replace(find, replace, comparisonType);
                                changed = true;
                            }
                            if (pageModule.Header != null && pageModule.Header.Contains(find, comparisonType))
                            {
                                pageModule.Header = pageModule.Header.Replace(find, replace, comparisonType);
                                changed = true;
                            }
                            if (pageModule.Footer != null && pageModule.Footer.Contains(find, comparisonType))
                            {
                                pageModule.Footer = pageModule.Footer.Replace(find, replace, comparisonType);
                                changed = true;
                            }
                            if (changed && globalReplace.Modules)
                            {
                                pageModuleRepository.UpdatePageModule(pageModule);
                                log += $"Module Updated: {pageModule.Title} - /{page.Path}<br />";
                            }

                            // module content
                            if (pageModule.Module.ModuleDefinition != null && pageModule.Module.ModuleDefinition.ServerManagerType != "")
                            {
                                Type moduleType = Type.GetType(pageModule.Module.ModuleDefinition.ServerManagerType);
                                if (moduleType != null && moduleType.GetInterface(nameof(IPortable)) != null)
                                {
                                    try
                                    {
                                        // module content
                                        var moduleObject = ActivatorUtilities.CreateInstance(provider, moduleType);
                                        var moduleContent = ((IPortable)moduleObject).ExportModule(pageModule.Module);
                                        if (!string.IsNullOrEmpty(moduleContent) && moduleContent.Contains(find, comparisonType) && globalReplace.Content)
                                        {
                                            moduleContent = moduleContent.Replace(find, replace, comparisonType);
                                            ((IPortable)moduleObject).ImportModule(pageModule.Module, moduleContent, pageModule.Module.ModuleDefinition.Version);
                                            log += $"Module Content Updated: {pageModule.Title} - /{page.Path}<br />";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log += $"Error Processing Module {pageModule.Module.ModuleDefinition.Name} - {ex.Message}<br />";
                                    }
                                }
                            }
                        }
                    }

                    // remove global replace setting to prevent reprocessing
                    settingRepository.DeleteSetting(EntityNames.Site, globalReplaceSettings.First().SettingId);
                }
                else
                {
                    log += $"No Criteria Provided<br />";
                }
            }

            return log;
        }
    }
}
