using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Modules.Admin.Modules;
using Oqtane.Shared;
using Module = Oqtane.Models.Module;

namespace Oqtane.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _factory;
        private readonly IRoleRepository _roleRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly IPageModuleRepository _pageModuleRepository;
        private readonly IModuleDefinitionRepository _moduleDefinitionRepository;
        private readonly IThemeRepository _themeRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigurationRoot _config;
        private readonly IServerStateManager _serverState;
        private readonly ILogManager _logger;
        private static readonly object _lock = new object();

        public SiteRepository(IDbContextFactory<TenantDBContext> factory, IRoleRepository roleRepository, IProfileRepository profileRepository, IFolderRepository folderRepository, IPageRepository pageRepository,
            IModuleRepository moduleRepository, IPageModuleRepository pageModuleRepository, IModuleDefinitionRepository moduleDefinitionRepository, IThemeRepository themeRepository, ISettingRepository settingRepository,
            IServiceProvider serviceProvider, IConfigurationRoot config, IServerStateManager serverState, ILogManager logger)
        {
            _factory = factory;
            _roleRepository = roleRepository;
            _profileRepository = profileRepository;
            _folderRepository = folderRepository;
            _pageRepository = pageRepository;
            _moduleRepository = moduleRepository;
            _pageModuleRepository = pageModuleRepository;
            _moduleDefinitionRepository = moduleDefinitionRepository;
            _themeRepository = themeRepository;
            _settingRepository = settingRepository;
            _serviceProvider = serviceProvider;
            _config = config;
            _serverState = serverState;
            _logger = logger;
        }

        public IEnumerable<Site> GetSites()
        {
            using var db = _factory.CreateDbContext();
            return db.Site.OrderBy(item => item.Name).ToList();
        }

        public Site AddSite(Site site)
        {
            using var db = _factory.CreateDbContext();
            site.SiteGuid = Guid.NewGuid().ToString();
            db.Site.Add(site);
            db.SaveChanges();
            CreateSite(site);
            return site;
        }

        public Site UpdateSite(Site site)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(site).State = EntityState.Modified;
            db.SaveChanges();
            return site;
        }

        public Site GetSite(int siteId)
        {
            return GetSite(siteId, true);
        }

        public Site GetSite(int siteId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.Site.Find(siteId);
            }
            else
            {
                return db.Site.AsNoTracking().FirstOrDefault(item => item.SiteId == siteId);
            }
        }

        public void DeleteSite(int siteId)
        {
            using var db = _factory.CreateDbContext();
            var site = db.Site.Find(siteId);
            db.Site.Remove(site);
            db.SaveChanges();
        }


        public void InitializeSite(Alias alias)
        {
            var serverstate = _serverState.GetServerState(alias.SiteKey);
            if (!serverstate.IsInitialized)
            {
                // ensure site initialization is only executed once
                lock (_lock)
                {
                    if (!serverstate.IsInitialized)
                    {
                        var site = GetSite(alias.SiteId);
                        if (site != null)
                        {
                            // initialize theme Assemblies
                            site.Themes = _themeRepository.GetThemes().ToList();

                            // initialize module Assemblies
                            var moduleDefinitions = _moduleDefinitionRepository.GetModuleDefinitions(alias.SiteId);

                            // execute migrations
                            var version = ProcessSiteMigrations(alias, site);
                            version = ProcessPageTemplates(alias, site, moduleDefinitions, version);
                            if (site.Version != version)
                            {
                                site.Version = version;
                                UpdateSite(site);
                            }
                        }
                        serverstate.IsInitialized = true;
                    }
                }
            } 
        }

        private string ProcessSiteMigrations(Alias alias, Site site)
        {
            var version = site.Version;
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes(typeof(ISiteMigration)))
                {
                    if (Attribute.IsDefined(type, typeof(SiteMigrationAttribute)))
                    {
                        var attribute = (SiteMigrationAttribute)Attribute.GetCustomAttribute(type, typeof(SiteMigrationAttribute));
                        if (attribute.AliasName == "*" || attribute.AliasName == alias.Name)
                        {
                            if (string.IsNullOrEmpty(site.Version) || Version.Parse(attribute.Version) > Version.Parse(site.Version))
                            {
                                try
                                {
                                    var obj = ActivatorUtilities.CreateInstance(_serviceProvider, type) as ISiteMigration;
                                    if (obj != null)
                                    {
                                        obj.Up(site, alias);
                                        _logger.Log(LogLevel.Information, "Site Migration", LogFunction.Other, "Site Migrated Successfully To Version {version} For {Alias}", version, alias.Name);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Log(LogLevel.Error, "Site Migration", LogFunction.Other, ex, "An Error Occurred Executing Site Migration {Type} For {Alias} And Version {Version}", type, alias.Name, version);
                                }
                                if (string.IsNullOrEmpty(version) || Version.Parse(attribute.Version) > Version.Parse(version))
                                {
                                    version = attribute.Version;
                                }
                            }
                        }
                    }
                }
            }
            return version;
        }

        private string ProcessPageTemplates(Alias alias, Site site, IEnumerable<ModuleDefinition> moduleDefinitions, string version)
        {
            var pageTemplates = new List<PageTemplate>();
            foreach (var moduleDefinition in moduleDefinitions)
            {
                if (moduleDefinition.PageTemplates != null)
                {
                    foreach (var pageTemplate in moduleDefinition.PageTemplates)
                    {
                        if (pageTemplate.PageTemplateModules.Count == 0)
                        {
                            pageTemplate.PageTemplateModules.Add(new PageTemplateModule());
                        }
                        foreach (var pageTemplateModule in pageTemplate.PageTemplateModules)
                        {
                            if (string.IsNullOrEmpty(pageTemplateModule.ModuleDefinitionName))
                            {
                                pageTemplateModule.ModuleDefinitionName = moduleDefinition.ModuleDefinitionName;
                            }
                            if (string.IsNullOrEmpty(pageTemplateModule.Title))
                            {
                                pageTemplateModule.Title = moduleDefinition.Name;
                            }
                        }
                        pageTemplates.Add(pageTemplate);
                        if (pageTemplate.Version != "*" && (string.IsNullOrEmpty(version) || Version.Parse(pageTemplate.Version) > Version.Parse(version)))
                        {
                            version = pageTemplate.Version;
                        }
                    }
                }
            }
            CreatePages(site, pageTemplates, alias);
            return version;
        }

        private void CreateSite(Site site)
        {
            // create default entities for site
            List<Role> roles = _roleRepository.GetRoles(site.SiteId, true).ToList();
            if (!roles.Where(item => item.Name == RoleNames.Everyone).Any())
            {
                _roleRepository.AddRole(new Role {SiteId = null, Name = RoleNames.Everyone, Description = RoleNames.Everyone, IsAutoAssigned = false, IsSystem = true});
            }
            if (!roles.Where(item => item.Name == RoleNames.Unauthenticated).Any())
            {
                _roleRepository.AddRole(new Role { SiteId = null, Name = RoleNames.Unauthenticated, Description = RoleNames.Unauthenticated, IsAutoAssigned = false, IsSystem = true });
            }
            if (!roles.Where(item => item.Name == RoleNames.Host).Any())
            {
                _roleRepository.AddRole(new Role {SiteId = null, Name = RoleNames.Host, Description = RoleNames.Host, IsAutoAssigned = false, IsSystem = true});
            }
            _roleRepository.AddRole(new Role {SiteId = site.SiteId, Name = RoleNames.Registered, Description = RoleNames.Registered, IsAutoAssigned = true, IsSystem = true});
            _roleRepository.AddRole(new Role {SiteId = site.SiteId, Name = RoleNames.Admin, Description = RoleNames.Admin, IsAutoAssigned = false, IsSystem = true});

            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "FirstName", Title = "First Name", Description = "Your First Or Given Name", Category = "Name", ViewOrder = 1, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "LastName", Title = "Last Name", Description = "Your Last Or Family Name", Category = "Name", ViewOrder = 2, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "Street", Title = "Street", Description = "Street Or Building Address", Category = "Address", ViewOrder = 3, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile 
            { SiteId = site.SiteId, Name = "City", Title = "City", Description = "City", Category = "Address", ViewOrder = 4, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "Region", Title = "Region", Description = "State Or Province", Category = "Address", ViewOrder = 5, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "Country", Title = "Country", Description = "Country", Category = "Address", ViewOrder = 6, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "PostalCode", Title = "Postal Code", Description = "Postal Code Or Zip Code", Category = "Address", ViewOrder = 7, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });
            _profileRepository.AddProfile(new Profile
            { SiteId = site.SiteId, Name = "Phone", Title = "Phone Number", Description = "Phone Number", Category = "Contact", ViewOrder = 8, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "", Rows = 1 });

            Folder folder = _folderRepository.AddFolder(new Folder
            {
                SiteId = site.SiteId, ParentId = null, Name = "Root", Type = FolderTypes.Private, Path = "", Order = 1, ImageSizes = "", Capacity = 0, IsSystem = true,
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.Browse, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }
            });
            _folderRepository.AddFolder(new Folder { SiteId = site.SiteId, ParentId = folder.FolderId, Name = "Public", Type = FolderTypes.Public, Path = "Public/", Order = 1, ImageSizes = "", Capacity = 0, IsSystem = false,
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.Browse, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }
            });
            _folderRepository.AddFolder(new Folder
            {
                SiteId = site.SiteId, ParentId = folder.FolderId, Name = "Users", Type = FolderTypes.Private, Path = "Users/", Order = 3, ImageSizes = "", Capacity = 0, IsSystem = true,
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.Browse, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }
            });

            // admin site template
            var siteTemplateType = Type.GetType(Constants.AdminSiteTemplate);
            var siteTemplateObject = ActivatorUtilities.CreateInstance(_serviceProvider, siteTemplateType);
            List<PageTemplate> adminPageTemplates = ((ISiteTemplate)siteTemplateObject).CreateSite(site);
            CreatePages(site, adminPageTemplates, null);

            // process site template
            if (string.IsNullOrEmpty(site.SiteTemplateType))
            {
                var section = _config.GetSection("Installation:SiteTemplate");
                if (section.Exists())
                {
                    if(string.IsNullOrEmpty(section.Value)){
                        site.SiteTemplateType = Constants.DefaultSiteTemplate;
                    }
                    else
                    {
                        site.SiteTemplateType = section.Value;
                    }                    
                }
                else
                {
                    site.SiteTemplateType = Constants.DefaultSiteTemplate;
                }
            }

            siteTemplateType = Type.GetType(site.SiteTemplateType);
            if (siteTemplateType != null)
            {
                siteTemplateObject = ActivatorUtilities.CreateInstance(_serviceProvider, siteTemplateType);
                List<PageTemplate> pageTemplates = ((ISiteTemplate) siteTemplateObject).CreateSite(site);
                if (pageTemplates != null && pageTemplates.Count > 0)
                {
                    CreatePages(site, pageTemplates, null);
                }
            }
        }

        public void CreatePages(Site site, List<PageTemplate> pageTemplates, Alias alias)
        {
            List<Page> pages = null;
            List<PageModule> pageModules = null;
            List<ModuleDefinition> moduleDefinitions = null;

            foreach (PageTemplate pageTemplate in pageTemplates)
            {
                if (pageTemplate.AliasName == "*" || alias == null || pageTemplate.AliasName == alias.Name)
                {
                    if (string.IsNullOrEmpty(site.Version) || pageTemplate.Version == "*" || Version.Parse(pageTemplate.Version) > Version.Parse(site.Version))
                    {
                        if (pages == null)
                        {
                            pages = _pageRepository.GetPages(site.SiteId).ToList();
                        }
                        Page parent = null;
                        if (string.IsNullOrEmpty(pageTemplate.Path))
                        {
                            if (!string.IsNullOrEmpty(pageTemplate.Parent))
                            {
                                parent = pages.FirstOrDefault(item => item.Path.ToLower() == pageTemplate.Parent.ToLower());
                            }
                            pageTemplate.Path = (parent != null) ? parent.Path + "/" + pageTemplate.Name : pageTemplate.Name;
                        }
                        pageTemplate.Path = (pageTemplate.Path.ToLower() == "home") ? "" : pageTemplate.Path;
                        pageTemplate.Path = (pageTemplate.Path == "/") ? "" : pageTemplate.Path;
                        var page = pages.FirstOrDefault(item => item.Path.ToLower() == pageTemplate.Path.ToLower());
                        if (page == null)
                        {
                            page = new Page();
                            page.SiteId = site.SiteId;
                            page.Path = pageTemplate.Path;
                        }
                        page.Name = pageTemplate.Name;
                        if (string.IsNullOrEmpty(page.Name))
                        {
                            page.Name = (pageTemplate.Path.Contains("/")) ? pageTemplate.Path.Substring(pageTemplate.Name.LastIndexOf("/") + 1) : pageTemplate.Path;
                        }
                        if (string.IsNullOrEmpty(pageTemplate.Parent))
                        {
                            if (pageTemplate.Path.Contains("/"))
                            {
                                parent = pages.FirstOrDefault(item => item.Path.ToLower() == pageTemplate.Path.Substring(0, pageTemplate.Path.LastIndexOf("/")).ToLower());
                            }
                        }
                        else
                        {
                            parent = pages.FirstOrDefault(item => item.Path.ToLower() == ((pageTemplate.Parent == "/") ? "" : pageTemplate.Parent.ToLower()));
                        }
                        page.ParentId = (parent != null) ? parent.PageId : null;
                        page.Path = page.Path.ToLower();
                        page.Title = pageTemplate.Title;
                        page.Order = pageTemplate.Order;
                        page.Url = pageTemplate.Url;
                        page.ThemeType = pageTemplate.ThemeType;
                        page.DefaultContainerType = pageTemplate.DefaultContainerType;
                        page.HeadContent = pageTemplate.HeadContent;
                        page.BodyContent = pageTemplate.BodyContent;
                        page.Icon = pageTemplate.Icon;
                        page.IsNavigation = pageTemplate.IsNavigation;
                        page.IsClickable = pageTemplate.IsClickable;
                        page.IsPersonalizable = pageTemplate.IsPersonalizable;
                        page.UserId = null;
                        page.IsDeleted = pageTemplate.IsDeleted;
                        page.PermissionList = pageTemplate.PermissionList;
                        try
                        {
                            if (page.PageId != 0)
                            {
                                if (pageTemplate.Update)
                                {
                                    page = _pageRepository.UpdatePage(page);
                                    if (alias != null)
                                    {
                                        _logger.Log(LogLevel.Information, "Site Template", LogFunction.Update, "Page Updated {Page}", page);
                                    }
                                    UpdateSettings(EntityNames.Page, page.PageId, pageTemplate.Settings);
                                }
                            }
                            else
                            {
                                page = _pageRepository.AddPage(page);
                                pages.Add(page);
                                if (alias != null)
                                {
                                    _logger.Log(LogLevel.Information, "Site Template", LogFunction.Create, "Page Added {Page}", page);
                                }
                                UpdateSettings(EntityNames.Page, page.PageId, pageTemplate.Settings);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (alias != null)
                            {
                                _logger.Log(LogLevel.Error, "Site Template", LogFunction.Other, ex, "Error Processing Page {Page}", page);
                            }
                        }

                        if (pageModules == null)
                        {
                            pageModules = _pageModuleRepository.GetPageModules(site.SiteId).ToList();
                        }
                        if (moduleDefinitions == null)
                        {
                            moduleDefinitions = _moduleDefinitionRepository.GetModuleDefinitions(site.SiteId).ToList();
                        }
                        foreach (PageTemplateModule pageTemplateModule in pageTemplate.PageTemplateModules)
                        {
                            var moduleDefinition = moduleDefinitions.Where(item => item.ModuleDefinitionName == pageTemplateModule.ModuleDefinitionName).FirstOrDefault();
                            if (moduleDefinition != null)
                            {
                                var pageModule = pageModules.FirstOrDefault(item => item.PageId == page.PageId && item.Module.ModuleDefinitionName == pageTemplateModule.ModuleDefinitionName && item.Title.ToLower() == pageTemplateModule.Title.ToLower());
                                if (pageModule == null)
                                {
                                    pageModule = new PageModule();
                                    pageModule.PageId = page.PageId;
                                    pageModule.Module = new Module();
                                    pageModule.Module.SiteId = site.SiteId;
                                    pageModule.Module.ModuleDefinitionName = pageTemplateModule.ModuleDefinitionName;
                                }
                                pageModule.Title = pageTemplateModule.Title;
                                pageModule.Pane = (string.IsNullOrEmpty(pageTemplateModule.Pane)) ? PaneNames.Default : pageTemplateModule.Pane;
                                pageModule.Order = (pageTemplateModule.Order == 0) ? 1 : pageTemplateModule.Order;
                                pageModule.ContainerType = pageTemplateModule.ContainerType;
                                pageModule.Header = pageTemplateModule.Header;
                                pageModule.Footer = pageTemplateModule.Footer;
                                pageModule.IsDeleted = pageTemplateModule.IsDeleted;
                                pageModule.Module.PermissionList = new List<Permission>();
                                foreach (var permission in pageTemplateModule.PermissionList)
                                {
                                    pageModule.Module.PermissionList.Add(permission.Clone());
                                }
                                pageModule.Module.AllPages = false;
                                pageModule.Module.IsDeleted = false;
                                try
                                {
                                    if (pageModule.ModuleId != 0)
                                    {
                                        if (pageTemplate.Update)
                                        {
                                            _moduleRepository.UpdateModule(pageModule.Module);
                                            _pageModuleRepository.UpdatePageModule(pageModule);
                                            if (alias != null)
                                            {
                                                _logger.Log(LogLevel.Information, "Site Template", LogFunction.Update, "Page Module Updated {PageModule}", pageModule);
                                            }
                                            UpdateSettings(EntityNames.Module, pageModule.Module.ModuleId, pageTemplateModule.Settings);
                                        }
                                        else
                                        {
                                            pageTemplateModule.Content = ""; // do not update content
                                        }
                                    }
                                    else
                                    {
                                        var module = _moduleRepository.AddModule(pageModule.Module);
                                        pageModule.ModuleId = module.ModuleId;
                                        pageModule.Module = null; // remove tracking
                                        _pageModuleRepository.AddPageModule(pageModule);
                                        pageModule.Module = module;
                                        pageModules.Add(pageModule);
                                        if (alias != null)
                                        {
                                            _logger.Log(LogLevel.Information, "Site Template", LogFunction.Create, "Page Module Added {PageModule}", pageModule);
                                        }
                                        UpdateSettings(EntityNames.Module, pageModule.Module.ModuleId, pageTemplateModule.Settings);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    if (alias != null)
                                    {
                                        _logger.Log(LogLevel.Error, "Site Template", LogFunction.Other, ex, "Error Processing Page Module {PageModule}", pageModule);
                                    }
                                }

                                if (pageTemplateModule.Content != "" && moduleDefinition.ServerManagerType != "")
                                {
                                    Type moduletype = Type.GetType(moduleDefinition.ServerManagerType);
                                    if (moduletype != null && moduletype.GetInterface(nameof(IPortable)) != null)
                                    {
                                        try
                                        {
                                            var module = _moduleRepository.GetModule(pageModule.ModuleId);
                                            if (module != null)
                                            {
                                                var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                                ((IPortable)moduleobject).ImportModule(module, pageTemplateModule.Content, moduleDefinition.Version);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            if (alias != null)
                                            {
                                                _logger.Log(LogLevel.Error, "Site Template", LogFunction.Other, ex, "Error Importing Content For {ModuleDefinitionName}", pageTemplateModule.ModuleDefinitionName);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (alias != null)
                                {
                                    _logger.Log(LogLevel.Error, "Site Template", LogFunction.Other, "Module Definition Does Not Exist {ModuleDefinitionName}", pageTemplateModule.ModuleDefinitionName);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSettings(string entityName, int entityId, List<Setting> templateSettings)
        {
            foreach (var templateSetting in templateSettings)
            {
                var setting = _settingRepository.GetSetting(entityName, entityId, templateSetting.SettingName);
                if (setting == null)
                {
                    templateSetting.EntityName = entityName;
                    templateSetting.EntityId = entityId;
                    _settingRepository.AddSetting(templateSetting);
                }
                else
                {
                    setting.SettingValue = templateSetting.SettingValue;
                    setting.IsPrivate = templateSetting.IsPrivate;
                    _settingRepository.UpdateSetting(setting);
                }
            }
        }
    }
}
