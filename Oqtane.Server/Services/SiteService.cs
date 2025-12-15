using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Oqtane.Documentation;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Shared;
using System.Globalization;
using Oqtane.Extensions;
using Oqtane.Managers;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ServerSiteService : ISiteService
    {
        private readonly ISiteRepository _sites;
        private readonly IPageRepository _pages;
        private readonly IThemeRepository _themes;
        private readonly IPageModuleRepository _pageModules;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly ILanguageRepository _languages;
        private readonly IUserManager _userManager;
        private readonly IUserPermissions _userPermissions;
        private readonly ISettingRepository _settings;
        private readonly ITenantManager _tenantManager;
        private readonly ISyncManager _syncManager;
        private readonly IConfigManager _configManager;
        private readonly ILogManager _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _private = "[PRIVATE]";

        public ServerSiteService(ISiteRepository sites, IPageRepository pages, IThemeRepository themes, IPageModuleRepository pageModules, IModuleDefinitionRepository moduleDefinitions, ILanguageRepository languages, IUserManager userManager, IUserPermissions userPermissions, ISettingRepository settings, ITenantManager tenantManager, ISyncManager syncManager, IConfigManager configManager, ILogManager logger, IMemoryCache cache, IHttpContextAccessor accessor)
        {
            _sites = sites;
            _pages = pages;
            _themes = themes;
            _pageModules = pageModules;
            _moduleDefinitions = moduleDefinitions;
            _languages = languages;
            _userManager = userManager;
            _userPermissions = userPermissions;
            _settings = settings;
            _tenantManager = tenantManager;
            _syncManager = syncManager;
            _configManager = configManager;
            _logger = logger;
            _cache = cache;
            _accessor = accessor;
        }

        public Task<List<Site>> GetSitesAsync()
        {
            List<Site> sites = new List<Site>();
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Host))
            {
                sites = _sites.GetSites().ToList();
            }
            return Task.FromResult(sites);
        }

        public Task<Site> GetSiteAsync(int siteId)
        {
            var alias = _tenantManager.GetAlias();
            var site = _cache.GetOrCreate($"site:{alias.SiteKey}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return GetSite(siteId);
            });

            // clone object so that cache is not mutated
            site = site.Clone();

            // trim site settings based on user permissions
            site.Settings = site.Settings
                .Where(item => !item.Value.StartsWith(_private) || _accessor.HttpContext.User.IsInRole(RoleNames.Admin))
                .ToDictionary(setting => setting.Key, setting => setting.Value.Replace(_private, ""));

            // trim pages based on user permissions
            var pages = new List<Page>();
            foreach (Page page in site.Pages)
            {
                if (!page.IsDeleted && _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.View, page.PermissionList) && (Utilities.IsEffectiveAndNotExpired(page.EffectiveDate, page.ExpiryDate) || _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.Edit, page.PermissionList)))
                {
                    page.Settings = page.Settings
                        .Where(item => !item.Value.StartsWith(_private) || _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.Edit, page.PermissionList))
                        .ToDictionary(setting => setting.Key, setting => setting.Value.Replace(_private, ""));
                    pages.Add(page);
                }
            }
            site.Pages = pages;

            // get language display name for user
            foreach (Language language in site.Languages)
            {
                language.Name = CultureInfo.GetCultureInfo(language.Code).DisplayName;
            }
            site.Languages = site.Languages.OrderBy(item => item.Name).ToList();

            // get user
            if (_accessor.HttpContext.User.IsAuthenticated())
            {
                site.User = _userManager.GetUser(_accessor.HttpContext.User.UserId(), site.SiteId);
            }

            return Task.FromResult(site);
        }

        private Site GetSite(int siteid)
        {
            var alias = _tenantManager.GetAlias();
            var site = _sites.GetSite(siteid);
            if (site != null && site.SiteId == alias.SiteId)
            {
                // site settings
                site.Settings = _settings.GetSettings(EntityNames.Site, site.SiteId, EntityNames.Host, -1)
                    .ToDictionary(setting => setting.SettingName, setting => (setting.IsPrivate ? _private : "") + setting.SettingValue);

                // populate file extensions 
                site.ImageFiles = site.Settings.ContainsKey("ImageFiles") && !string.IsNullOrEmpty(site.Settings["ImageFiles"])
                    ? site.Settings["ImageFiles"] : Constants.ImageFiles;
                site.UploadableFiles = site.Settings.ContainsKey("UploadableFiles") && !string.IsNullOrEmpty(site.Settings["UploadableFiles"])
                    ? site.Settings["UploadableFiles"] : Constants.UploadableFiles;

                // pages
                List<Setting> settings = _settings.GetSettings(EntityNames.Page).ToList();
                site.Pages = new List<Page>();
                foreach (Page page in _pages.GetPages(site.SiteId))
                {
                    page.Settings = settings.Where(item => item.EntityId == page.PageId)
                        .ToDictionary(setting => setting.SettingName, setting => (setting.IsPrivate ? _private : "") + setting.SettingValue);
                    site.Pages.Add(page);
                }

                // framework modules
                var modules = GetPageModules(site.SiteId);
                site.Settings.Add(Constants.AdminDashboardModule, modules.FirstOrDefault(item => item.ModuleDefinitionName == Constants.AdminDashboardModule).ModuleId.ToString());
                site.Settings.Add(Constants.PageManagementModule, modules.FirstOrDefault(item => item.ModuleDefinitionName == Constants.PageManagementModule).ModuleId.ToString());

                // languages
                site.Languages = _languages.GetLanguages(site.SiteId).ToList();
                var defaultCulture = CultureInfo.GetCultureInfo(Constants.DefaultCulture);
                if (!site.Languages.Exists(item => item.Code == defaultCulture.Name))
                {
                    site.Languages.Add(new Language { Code = defaultCulture.Name, Name = "", Version = Constants.Version, IsDefault = !site.Languages.Any(l => l.IsDefault) });
                }

                // themes
                site.Themes = _themes.FilterThemes(_themes.GetThemes(site.SiteId).ToList());

                // installation date used for fingerprinting static assets
                site.Fingerprint = Utilities.GenerateSimpleHash(_configManager.GetSetting("InstallationDate", DateTime.UtcNow.ToString("yyyyMMddHHmm")));

                site.TenantId = alias.TenantId;
            }
            else
            {
                if (site != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Get Attempt {SiteId}", siteid);
                    site = null;
                }
            }
            return site;
        }

        public Task<Site> AddSiteAsync(Site site)
        {
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Host))
            {
                site = _sites.AddSite(site);
                _syncManager.AddSyncEvent(_tenantManager.GetAlias(), EntityNames.Site, site.SiteId, SyncEventActions.Create);
                _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Create, "Site Added {Site}", site);
            }
            else
            {
                site = null;
            }
            return Task.FromResult(site);
        }

        public Task<Site> UpdateSiteAsync(Site site)
        {
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Admin))
            {
                var alias = _tenantManager.GetAlias();
                var current = _sites.GetSite(site.SiteId, false);
                if (site.SiteId == alias.SiteId && current != null)
                {
                    site = _sites.UpdateSite(site);
                    _syncManager.AddSyncEvent(alias, EntityNames.Site, site.SiteId, SyncEventActions.Update);
                    string action = SyncEventActions.Refresh;
                    if (current.RenderMode != site.RenderMode || current.Runtime != site.Runtime)
                    {
                        action = SyncEventActions.Reload;
                    }
                    _syncManager.AddSyncEvent(alias, EntityNames.Site, site.SiteId, action);
                    _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Update, "Site Updated {Site}", site);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Put Attempt {Site}", site);
                    site = null;
                }
            }
            else
            {
                site = null;
            }
            return Task.FromResult(site);
        }

        public Task DeleteSiteAsync(int siteId)
        {
            if (_accessor.HttpContext.User.IsInRole(RoleNames.Host))
            {
                var alias = _tenantManager.GetAlias();
                var site = _sites.GetSite(siteId);
                if (site != null && site.SiteId == alias.SiteId)
                {
                    _sites.DeleteSite(siteId);
                    _syncManager.AddSyncEvent(alias, EntityNames.Site, site.SiteId, SyncEventActions.Delete);
                    _syncManager.AddSyncEvent(alias, EntityNames.Site, site.SiteId, SyncEventActions.Refresh);
                    _logger.Log(siteId, LogLevel.Information, this, LogFunction.Delete, "Site Deleted {SiteId}", siteId);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Delete Attempt {SiteId}", siteId);
                }
            }
            return Task.CompletedTask;
        }

        public Task<List<Module>> GetModulesAsync(int siteId, int pageId)
        {
            var alias = _tenantManager.GetAlias();
            var modules = _cache.GetOrCreate($"modules:{alias.SiteKey}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return GetPageModules(siteId);
            });

            // clone object so that cache is not mutated
            modules = modules.ConvertAll(module => module.Clone());

            // trim modules for current page based on user permissions
            var pagemodules = new List<Module>();
            foreach (Module module in modules.Where(item => (item.PageId == pageId || pageId == -1) && !item.IsDeleted && _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.View, item.PermissionList)))
            {
                if (Utilities.IsEffectiveAndNotExpired(module.EffectiveDate, module.ExpiryDate) || _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.Edit, module.PermissionList))
                {
                    module.Settings = module.Settings
                        .Where(item => !item.Value.StartsWith(_private) || _userPermissions.IsAuthorized(_accessor.HttpContext.User, PermissionNames.Edit, module.PermissionList))
                        .ToDictionary(setting => setting.Key, setting => setting.Value.Replace(_private, ""));
                    pagemodules.Add(module);
                }
            }
            return Task.FromResult(pagemodules);
        }

        private List<Module> GetPageModules(int siteId)
        {
            List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(siteId).ToList();
            var settings = _settings.GetSettings(EntityNames.Module).ToList();
            var modules = new List<Module>();

            foreach (PageModule pagemodule in _pageModules.GetPageModules(siteId))
            {
                Module module = new Module
                {
                    SiteId = pagemodule.Module.SiteId,
                    ModuleDefinitionName = pagemodule.Module.ModuleDefinitionName,
                    AllPages = pagemodule.Module.AllPages,
                    PermissionList = pagemodule.Module.PermissionList,
                    CreatedBy = pagemodule.Module.CreatedBy,
                    CreatedOn = pagemodule.Module.CreatedOn,
                    ModifiedBy = pagemodule.Module.ModifiedBy,
                    ModifiedOn = pagemodule.Module.ModifiedOn,
                    DeletedBy = pagemodule.DeletedBy,
                    DeletedOn = pagemodule.DeletedOn,
                    IsDeleted = pagemodule.IsDeleted,

                    PageModuleId = pagemodule.PageModuleId,
                    ModuleId = pagemodule.ModuleId,
                    PageId = pagemodule.PageId,
                    Title = pagemodule.Title,
                    Pane = pagemodule.Pane,
                    Order = pagemodule.Order,
                    ContainerType = pagemodule.ContainerType,
                    EffectiveDate = pagemodule.EffectiveDate,
                    ExpiryDate = pagemodule.ExpiryDate,
                    Header = pagemodule.Header,
                    Footer = pagemodule.Footer,

                    ModuleDefinition = _moduleDefinitions.FilterModuleDefinition(moduledefinitions.Find(item => item.ModuleDefinitionName == pagemodule.Module.ModuleDefinitionName)),

                    Settings = settings.Where(item => item.EntityId == pagemodule.ModuleId)
                        .ToDictionary(setting => setting.SettingName, setting => (setting.IsPrivate ? _private : "") + setting.SettingValue)
                };

                modules.Add(module);
            }

            return modules.OrderBy(item => item.PageId).ThenBy(item => item.Pane).ThenBy(item => item.Order).ToList();
        }

        [Obsolete("This method is deprecated.", false)]
        public void SetAlias(Alias alias)
        {
        }
    }
}
