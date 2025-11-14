using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Oqtane.Infrastructure.SiteTemplates;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Oqtane.Infrastructure
{
    public interface IUpgradeManager
    {
        void Upgrade(Tenant tenant, string version);
    }
    public class UpgradeManager : IUpgradeManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;
        private readonly ILogger<UpgradeManager> _filelogger;

        public UpgradeManager(IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment, IConfigManager configManager, ILogger<UpgradeManager> filelogger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
            _configManager = configManager;
            _filelogger = filelogger;
        }

        public void Upgrade(Tenant tenant, string version)
        {
            // core framework upgrade logic - executed for every tenant
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // set tenant
                var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
                tenantManager.SetTenant(tenant.TenantId);

                switch (version)
                {
                    case "2.0.2":
                        Upgrade_2_0_2(tenant, scope);
                        break;
                    case "2.1.0":
                        Upgrade_2_1_0(tenant, scope);
                        break;
                    case "2.2.0":
                        Upgrade_2_2_0(tenant, scope);
                        break;
                    case "3.0.1":
                        Upgrade_3_0_1(tenant, scope);
                        break;
                    case "3.1.3":
                        Upgrade_3_1_3(tenant, scope);
                        break;
                    case "3.1.4":
                        Upgrade_3_1_4(tenant, scope);
                        break;
                    case "3.2.0":
                        Upgrade_3_2_0(tenant, scope);
                        break;
                    case "3.2.1":
                        Upgrade_3_2_1(tenant, scope);
                        break;
                    case "3.3.0":
                        Upgrade_3_3_0(tenant, scope);
                        break;
                    case "5.1.0":
                        Upgrade_5_1_0(tenant, scope);
                        break;
                    case "5.2.1":
                        Upgrade_5_2_1(tenant, scope);
                        break;
                    case "6.1.0":
                        Upgrade_6_1_0(tenant, scope);
                        break;
                    case "6.1.1":
                        Upgrade_6_1_1(tenant, scope);
                        break;
                    case "6.1.5":
                        Upgrade_6_1_5(tenant, scope);
                        break;
                    case "6.2.0":
                        Upgrade_6_2_0(tenant, scope);
                        break;
                    case "6.2.1":
                        Upgrade_6_2_1(tenant, scope);
                        break;
                }
            }
        }

        private void Upgrade_2_0_2(Tenant tenant, IServiceScope scope)
        {
            // initialize SiteGuid
            try
            {
                var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                foreach (Site site in sites.GetSites().ToList())
                {
                    site.SiteGuid = System.Guid.NewGuid().ToString();
                    sites.UpdateSite(site);
                }
            }
            catch (Exception ex)
            {
                // error populating guid
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error In 2.0.2 Upgrade Logic - {ex}"));
            }
        }

        private void Upgrade_2_1_0(Tenant tenant, IServiceScope scope)
        {
            if (tenant.Name == TenantNames.Master)
            {
                _configManager.RemoveSetting("Localization:SupportedCultures", true);
                if (_configManager.GetSetting("RenderMode", "") == "")
                {
                    _configManager.AddOrUpdateSetting("RenderMode", RenderModes.Interactive, true);
                }
                if (_configManager.GetSetting("Runtime", "") == "")
                {
                    _configManager.AddOrUpdateSetting("Runtime", Runtimes.Server, true);
                }
            }
        }

        private void Upgrade_2_2_0(Tenant tenant, IServiceScope scope)
        {
            if (tenant.Name == TenantNames.Master)
            {
                if (_configManager.GetSetting("Logging:LogLevel:Default", "") == "")
                {
                    _configManager.AddOrUpdateSetting("Logging:LogLevel:Default", "Information", true);
                }
            }
        }

        private void Upgrade_3_0_1(Tenant tenant, IServiceScope scope)
        {
            var pageTemplates = new List<PageTemplate>
            {
                new PageTemplate
                {
                    Update = false,
                    Name = "Url Mappings",
                    Parent = "Admin",
                    Order = 33,
                    Path = "admin/urlmappings",
                    Icon = Icons.LinkBroken,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule>
                    {
                        new PageTemplateModule
                        {
                            ModuleDefinitionName = typeof(Oqtane.Modules.Admin.UrlMappings.Index).ToModuleDefinitionName(), Title = "Url Mappings", Pane = PaneNames.Default,
                            PermissionList = new List<Permission>
                            {
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            },
                            Content = ""
                        }
                    }
                },
                new PageTemplate
                {
                    Update = false,
                    Name = "Visitor Management",
                    Parent = "Admin",
                    Order = 35,
                    Path = "admin/visitors",
                    Icon = Icons.Eye,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule>
                    {
                        new PageTemplateModule
                        {
                            ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Visitors.Index).ToModuleDefinitionName(), Title = "Visitor Management", Pane = PaneNames.Default,
                            PermissionList = new List<Permission>
                            {
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            },
                            Content = ""
                        }
                    }
                }
            };

            AddPagesToSites(scope, tenant, pageTemplates);
        }

        private void Upgrade_3_1_3(Tenant tenant, IServiceScope scope)
        {
            var roles = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
            if (!roles.GetRoles(-1, true).ToList().Where(item => item.Name == RoleNames.Unauthenticated).Any())
            {
                roles.AddRole(new Role { SiteId = null, Name = RoleNames.Unauthenticated, Description = RoleNames.Unauthenticated, IsAutoAssigned = false, IsSystem = true });
            }
        }

        private void Upgrade_3_1_4(Tenant tenant, IServiceScope scope)
        {
            var pageTemplates = new List<PageTemplate>();

            pageTemplates.Add(new PageTemplate
            {
                Name = "Not Found",
                Parent = "",
                Path = "404",
                Icon = Icons.X,
                IsNavigation = false,
                IsPersonalizable = false,
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Not Found", Pane = PaneNames.Default,
                        PermissionList = new List<Permission> {
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
                        Content = "<p>The page you requested does not exist or you do not have sufficient rights to view it.</p>"
                    }
                }
            });

            var pages = scope.ServiceProvider.GetRequiredService<IPageRepository>();

            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
            foreach (Site site in sites.GetSites().ToList())
            {
                if (!pages.GetPages(site.SiteId).ToList().Where(item => item.Path == "404").Any())
                {
                    sites.CreatePages(site, pageTemplates, null);
                }
            }
        }

        private void Upgrade_3_2_0(Tenant tenant, IServiceScope scope)
        {
            try
            {
                // convert folder paths to cross platform format
                var siteRepository = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                var folderRepository = scope.ServiceProvider.GetRequiredService<IFolderRepository>();
                foreach (Site site in siteRepository.GetSites().ToList())
                {
                    foreach (Folder folder in folderRepository.GetFolders(site.SiteId).ToList())
                    {
                        folder.Path = folder.Path.Replace("\\", "/");
                        folderRepository.UpdateFolder(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error In 3.2.0 Upgrade Logic - {ex}"));
            }
        }

        private void Upgrade_3_2_1(Tenant tenant, IServiceScope scope)
        {
            try
            {
                // convert Identifier Claim Type and Email Claim Type
                var settingRepository = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
                var siteRepository = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                foreach (Site site in siteRepository.GetSites().ToList())
                {
                    var settings = settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToList();
                    var setting = settings.FirstOrDefault(item => item.SettingName == "ExternalLogin:IdentifierClaimType");
                    if (setting != null)
                    {
                        if (setting.SettingValue == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                        {
                            setting.SettingValue = "sub";
                            settingRepository.UpdateSetting(setting);
                        }
                    }
                    setting = settings.FirstOrDefault(item => item.SettingName == "ExternalLogin:EmailClaimType");
                    if (setting != null)
                    {
                        if (setting.SettingValue == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                        {
                            setting.SettingValue = "email";
                            settingRepository.UpdateSetting(setting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error In 3.2.1 Upgrade Logic - {ex}"));
            }
        }

        private void Upgrade_3_3_0(Tenant tenant, IServiceScope scope)
        {
            try
            {
                var roles = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
                var pages = scope.ServiceProvider.GetRequiredService<IPageRepository>();
                var modules = scope.ServiceProvider.GetRequiredService<IModuleRepository>();
                var permissions = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
                var siteRepository = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                foreach (Site site in siteRepository.GetSites().ToList())
                {
                    int roleid = roles.GetRoles(site.SiteId).FirstOrDefault(item => item.Name == RoleNames.Registered).RoleId;

                    int pageid = pages.GetPages(site.SiteId).FirstOrDefault(item => item.Path == "admin").PageId;
                    var permission = new Permission
                    {
                        SiteId = site.SiteId,
                        EntityName = EntityNames.Page,
                        EntityId = pageid,
                        PermissionName = PermissionNames.View,
                        RoleId = roleid,
                        IsAuthorized = true
                    };
                    permissions.AddPermission(permission);

                    int moduleid = modules.GetModules(site.SiteId).FirstOrDefault(item => item.ModuleDefinitionName == "Oqtane.Modules.Admin.Dashboard, Oqtane.Client").ModuleId;
                    permission = new Permission
                    {
                        SiteId = site.SiteId,
                        EntityName = EntityNames.Module,
                        EntityId = moduleid,
                        PermissionName = PermissionNames.View,
                        RoleId = roleid,
                        IsAuthorized = true
                    };
                    permissions.AddPermission(permission);
                }
            }
            catch (Exception ex)
            {
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error In 3.3.0 Upgrade Logic - {ex}"));
            }
        }

        private void Upgrade_5_1_0(Tenant tenant, IServiceScope scope)
        {
            if (tenant.Name == TenantNames.Master)
            {
                var rendermode = _configManager.GetSetting("RenderMode", "");
                if (rendermode.Contains("Prerendered"))
                {
                    _configManager.AddOrUpdateSetting("RenderMode", rendermode.Replace("Prerendered", ""), true);
                }

                try
                {
                    // delete legacy Views assemblies which will cause startup errors due to missing HostModel
                    // note that the following files will be deleted however the framework has already started up so another restart will be required
                    var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var filepath = Path.Combine(binFolder, "Oqtane.Server.Views.dll");
                    if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    filepath = Path.Combine(binFolder, "Oqtane.Server.Views.pdb");
                    if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                }
                catch (Exception ex)
                {
                    // error deleting file
                    _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error In 5.1.0 Upgrade Logic - {ex}"));
                }
            }
        }

        private void Upgrade_5_2_1(Tenant tenant, IServiceScope scope)
        {
            var pageTemplates = new List<PageTemplate>
            {
                new PageTemplate
                {
                    Update = false,
                    Name = "Search",
                    Parent = "",
                    Path = "search",
                    Icon = Icons.MagnifyingGlass,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission> {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule> {
                        new PageTemplateModule { ModuleDefinitionName = typeof(Oqtane.Modules.Admin.SearchResults.Index).ToModuleDefinitionName(), Title = "Search", Pane = PaneNames.Default,
                            PermissionList = new List<Permission> {
                                new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            }
                        }
                    }
                },
                new PageTemplate
                {
                    Update = false,
                    Name = "Search Settings",
                    Parent = "",
                    Path = "admin/search",
                    Icon = Icons.MagnifyingGlass,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission> {
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule> {
                        new PageTemplateModule { ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Search.Index).ToModuleDefinitionName(), Title = "Search Settings", Pane = PaneNames.Default,
                            PermissionList = new List<Permission> {
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            }
                        }
                    }
                }
            };

            AddPagesToSites(scope, tenant, pageTemplates);
        }

        private void Upgrade_6_1_0(Tenant tenant, IServiceScope scope)
        {
            // remove MySql.EntityFrameworkCore package (replaced by Pomelo.EntityFrameworkCore.MySql)
            string[] assemblies = {
                "MySql.EntityFrameworkCore.dll"
            };

            RemoveAssemblies(tenant, assemblies, "6.1.0");
        }

        private void Upgrade_6_1_1(Tenant tenant, IServiceScope scope)
        {
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<AdminSiteTemplate>>();

            var pageTemplates = new List<PageTemplate>
            {
                new PageTemplate
                {
                    Name = "Privacy",
                    Parent = "",
                    Path = "privacy",
                    Order = 1011,
                    Icon = Icons.Eye,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule>
                    {
                        new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Privacy Policy", Pane = PaneNames.Default,
                            PermissionList = new List<Permission> {
                                new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            },
                            Settings = new List<Setting> {
                                new Setting { SettingName = "DynamicTokens", SettingValue = "true" }
                            },
                            Content = localizer["Privacy"]
                        }
                    }
                },
                new PageTemplate
                {
                    Name = "Terms",
                    Parent = "",
                    Path = "terms",
                    Order = 1013,
                    Icon = Icons.List,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule>
                    {
                        new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Terms of Use", Pane = PaneNames.Default,
                            PermissionList = new List<Permission> {
                                new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                new Permission(PermissionNames.View, RoleNames.Admin, true),
                                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                            },
                            Settings = new List<Setting> {
                                new Setting { SettingName = "DynamicTokens", SettingValue = "true" }
                            },
                            Content = localizer["Terms"]
                        }
                    }
                }
            };

            AddPagesToSites(scope, tenant, pageTemplates);
        }

        private void Upgrade_6_1_5(Tenant tenant, IServiceScope scope)
        {
            // remove Database Providers which were moved to Oqtane.Server
            string[] assemblies = {
                "Oqtane.Database.MySQL.dll",
                "Oqtane.Database.MySQL.pdb",
                "Oqtane.Database.PostgreSQL.dll",
                "Oqtane.Database.PostgreSQL.pdb",
                "Oqtane.Database.Sqlite.dll",
                "Oqtane.Database.Sqlite.pdb",
                "Oqtane.Database.SqlServer.dll",
                "Oqtane.Database.SqlServer.pdb"
            };

            RemoveAssemblies(tenant, assemblies, "6.1.5");
        }

        private void Upgrade_6_2_0(Tenant tenant, IServiceScope scope)
        {
            var pageTemplates = new List<PageTemplate>
            {
                new PageTemplate
                {
                    Update = false,
                    Name = "Setting Management",
                    Parent = "Admin",
                    Order = 67,
                    Path = "admin/settings",
                    Icon = Icons.Cog,
                    IsNavigation = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Host, true),
                        new Permission(PermissionNames.Edit, RoleNames.Host, true)
                    },
                    PageTemplateModules = new List<PageTemplateModule>
                    {
                        new PageTemplateModule
                        {
                            ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Settings.Index).ToModuleDefinitionName(), Title = "Setting Management", Pane = PaneNames.Default,
                            PermissionList = new List<Permission>
                            {
                                new Permission(PermissionNames.View, RoleNames.Host, true),
                                new Permission(PermissionNames.Edit, RoleNames.Host, true)
                            },
                            Content = ""
                        }
                    }
                }
            };

            AddPagesToSites(scope, tenant, pageTemplates);
        }

        private void Upgrade_6_2_1(Tenant tenant, IServiceScope scope)
        {
            // remove text editor files moved to new location
            string[] files = {
                "js/quill.min.js.map",
                "js/quill1.3.7.min.js",
                "js/quill.min.js",
                "js/quill-blot-formatter.min.js",
                "js/quill-interop.js",
                "css/quill/quill1.3.7.bubble.css",
                "css/quill/quill.bubble.css",
                "css/quill/quill1.3.7.snow.css",
                "css/quill/quill.snow.css",
                "oqtane-black.png"
            };

            RemoveFiles(tenant, files, "6.2.1");
        }

        private void AddPagesToSites(IServiceScope scope, Tenant tenant, List<PageTemplate> pageTemplates)
        {
            var tenants = scope.ServiceProvider.GetRequiredService<ITenantManager>();
            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
            foreach (var site in sites.GetSites().ToList())
            {
                tenants.SetAlias(tenant.TenantId, site.SiteId);
                sites.CreatePages(site, pageTemplates, null);
            }
        }

        private void RemoveAssemblies(Tenant tenant, string[] assemblies, string version)
        {
            // in a development environment assemblies cannot be removed as the debugger runs fron /bin folder and locks the files
            if (tenant.Name == TenantNames.Master && !_environment.IsDevelopment())
            {
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var bin = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                        var filepath = Path.Combine(bin, assembly);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch (Exception ex)
                    {
                        // error deleting asesmbly
                        _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: {version} Upgrade Error Removing {assembly} - {ex}"));
                    }
                }
            }
        }

        private void RemoveFiles(Tenant tenant, string[] files, string version)
        {
            if (tenant.Name == TenantNames.Master)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var wwwroot = _environment.WebRootPath;
                        var filepath = Path.Combine(wwwroot, file);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch (Exception ex)
                    {
                        // error deleting file
                        _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: {version} Upgrade Error Removing {file} - {ex}"));
                    }
                }
            }
        }
    }
}
