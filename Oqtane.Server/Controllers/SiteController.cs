using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;
using Oqtane.Security;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Extensions;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SiteController : Controller
    {
        private readonly ISiteRepository _sites;
        private readonly IPageRepository _pages;
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly ILanguageRepository _languages;
        private readonly IUserPermissions _userPermissions;
        private readonly ISettingRepository _settings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly IMemoryCache _cache;
        private readonly Alias _alias;

        public SiteController(ISiteRepository sites, IPageRepository pages, IModuleRepository modules, IPageModuleRepository pageModules, IModuleDefinitionRepository moduleDefinitions, ILanguageRepository languages, IUserPermissions userPermissions, ISettingRepository settings, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger, IMemoryCache cache)
        {
            _sites = sites;
            _pages = pages;
            _modules = modules;
            _pageModules = pageModules;
            _moduleDefinitions = moduleDefinitions;
            _languages = languages;
            _userPermissions = userPermissions;
            _settings = settings;
            _syncManager = syncManager;
            _logger = logger;
            _cache = cache;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<Site> Get()
        {
            return _sites.GetSites();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Site Get(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return _cache.GetOrCreate($"site:{HttpContext.GetAlias().SiteKey}", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return GetSite(id);
                });
            }
            else
            {
                return GetSite(id);
            }
        }

        private Site GetSite(int siteid)
        {
            var site = _sites.GetSite(siteid);
            if (site.SiteId == _alias.SiteId)
            {
                // site settings
                site.Settings = _settings.GetSettings(EntityNames.Site, site.SiteId)
                    .Where(item => !item.IsPrivate || User.IsInRole(RoleNames.Admin))
                    .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);

                // pages
                List<Setting> settings = _settings.GetSettings(EntityNames.Page).ToList();
                site.Pages = new List<Page>();
                foreach (Page page in _pages.GetPages(site.SiteId))
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.View, page.PermissionList))
                    {
                        page.Settings = settings.Where(item => item.EntityId == page.PageId)
                            .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, page.PermissionList))
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                        site.Pages.Add(page);
                    }
                }
                site.Pages = GetPagesHierarchy(site.Pages);

                // modules
                List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(site.SiteId).ToList();
                settings = _settings.GetSettings(EntityNames.Module).ToList();
                site.Modules = new List<Module>();
                foreach (PageModule pagemodule in _pageModules.GetPageModules(site.SiteId))
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.View, pagemodule.Module.PermissionList))
                    {
                        Module module = new Module();
                        module.SiteId = pagemodule.Module.SiteId;
                        module.ModuleDefinitionName = pagemodule.Module.ModuleDefinitionName;
                        module.AllPages = pagemodule.Module.AllPages;
                        module.PermissionList = pagemodule.Module.PermissionList;
                        module.CreatedBy = pagemodule.Module.CreatedBy;
                        module.CreatedOn = pagemodule.Module.CreatedOn;
                        module.ModifiedBy = pagemodule.Module.ModifiedBy;
                        module.ModifiedOn = pagemodule.Module.ModifiedOn;
                        module.DeletedBy = pagemodule.DeletedBy;
                        module.DeletedOn = pagemodule.DeletedOn;
                        module.IsDeleted = pagemodule.IsDeleted;

                        module.PageModuleId = pagemodule.PageModuleId;
                        module.ModuleId = pagemodule.ModuleId;
                        module.PageId = pagemodule.PageId;
                        module.Title = pagemodule.Title;
                        module.Pane = pagemodule.Pane;
                        module.Order = pagemodule.Order;
                        module.ContainerType = pagemodule.ContainerType;

                        module.ModuleDefinition = _moduleDefinitions.FilterModuleDefinition(moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName));

                        module.Settings = settings.Where(item => item.EntityId == pagemodule.ModuleId)
                            .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, pagemodule.Module.PermissionList))
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);

                        site.Modules.Add(module);
                    }
                }
                site.Modules = site.Modules.OrderBy(item => item.PageId).ThenBy(item => item.Pane).ThenBy(item => item.Order).ToList();

                // languages
                site.Languages = _languages.GetLanguages(site.SiteId).ToList();
                var defaultCulture = CultureInfo.GetCultureInfo(Constants.DefaultCulture);
                site.Languages.Add(new Language { Code = defaultCulture.Name, Name = defaultCulture.DisplayName, Version = Constants.Version, IsDefault = !site.Languages.Any(l => l.IsDefault) });

                return site;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public Site Post([FromBody] Site site)
        {
            if (ModelState.IsValid)
            {
                site = _sites.AddSite(site);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, site.SiteId, SyncEventActions.Create);
                _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Create, "Site Added {Site}", site);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Post Attempt {Site}", site);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                site = null;
            }
            return site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Site Put(int id, [FromBody] Site site)
        {
            var current = _sites.GetSite(site.SiteId, false);
            if (ModelState.IsValid && site.SiteId == _alias.SiteId && site.TenantId == _alias.TenantId && current != null)
            {
                site = _sites.UpdateSite(site);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, site.SiteId, SyncEventActions.Update);
                string action = SyncEventActions.Refresh;
                if (current.Runtime != site.Runtime || current.RenderMode != site.RenderMode)
                {
                    action = SyncEventActions.Reload;
                }
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, site.SiteId, action);
                _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Update, "Site Updated {Site}", site);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Put Attempt {Site}", site);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                site = null;
            }
            return site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            var site = _sites.GetSite(id);
            if (site != null && site.SiteId == _alias.SiteId)
            {
                _sites.DeleteSite(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, site.SiteId, SyncEventActions.Delete);
                _logger.Log(id, LogLevel.Information, this, LogFunction.Delete, "Site Deleted {SiteId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Delete Attempt {SiteId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private static List<Page> GetPagesHierarchy(List<Page> pages)
        {
            List<Page> hierarchy = new List<Page>();
            Action<List<Page>, Page> getPath = null;
            getPath = (pageList, page) =>
            {
                IEnumerable<Page> children;
                int level;
                if (page == null)
                {
                    level = -1;
                    children = pages.Where(item => item.ParentId == null);
                }
                else
                {
                    level = page.Level;
                    children = pages.Where(item => item.ParentId == page.PageId);
                }
                foreach (Page child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = pages.Any(item => item.ParentId == child.PageId);
                    hierarchy.Add(child);
                    getPath(pageList, child);
                }
            };
            pages = pages.OrderBy(item => item.Order).ToList();
            getPath(pages, null);

            // add any non-hierarchical items to the end of the list
            foreach (Page page in pages)
            {
                if (hierarchy.Find(item => item.PageId == page.PageId) == null)
                {
                    hierarchy.Add(page);
                }
            }
            return hierarchy;
        }
    }
}
