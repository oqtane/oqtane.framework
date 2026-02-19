using System;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class GlobalReplaceTask : SiteTaskBase
    {
        public override string ExecuteTask(IServiceProvider provider, Site site, string parameters)
        {
            string log = "";

            // get services
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var pageRepository = provider.GetRequiredService<IPageRepository>();
            var pageModuleRepository = provider.GetRequiredService<IPageModuleRepository>();

            if (!string.IsNullOrEmpty(parameters))
            {
                // get parameters
                var globalReplace = JsonSerializer.Deserialize<GlobalReplace>(parameters);

                var find = globalReplace.Find;
                var replace = globalReplace.Replace;
                var comparisonType = (globalReplace.CaseSensitive) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                log += $"Replacing: '{find}' With: '{replace}' Case Sensitive: {globalReplace.CaseSensitive}<br />";

                // site properties
                site = siteRepository.GetSite(site.SiteId);
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

                var pages = pageRepository.GetPages(site.SiteId).ToList();
                var pageModules = pageModuleRepository.GetPageModules(site.SiteId).ToList();

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
                        // module properties
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
            }
            else
            {
                log += $"No Criteria Provided<br />";
            }

            return log;
        }
    }
}
