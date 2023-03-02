using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public class UpgradeManager : IUpgradeManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;

        public UpgradeManager(IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment, IConfigManager configManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
            _configManager = configManager;
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
                }
            }
        }

        private void Upgrade_2_0_2(Tenant tenant, IServiceScope scope)
        {
            if (tenant.Name == TenantNames.Master)
            {
                // remove Internal module template files as they are no longer supported
                var internalTemplatePath = Utilities.PathCombine(_environment.WebRootPath, "Modules", "Templates", "Internal", Path.DirectorySeparatorChar.ToString());
                if (Directory.Exists(internalTemplatePath))
                {
                    try
                    {
                        Directory.Delete(internalTemplatePath, true);
                    }
                    catch (Exception ex)
                    {
                        // error deleting directory
                        Debug.WriteLine($"Oqtane Error: Error In 2.0.2 Upgrade Logic - {ex}");
                    }
                }
            }

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
                Debug.WriteLine($"Oqtane Error: Error In 2.0.2 Upgrade Logic - {ex}");
            }
        }

        private void Upgrade_2_1_0(Tenant tenant, IServiceScope scope)
        {
            if (tenant.Name == TenantNames.Master)
            {
                _configManager.RemoveSetting("Localization:SupportedCultures", true);
                if (_configManager.GetSetting("RenderMode", "") == "")
                {
                    _configManager.AddOrUpdateSetting("RenderMode", "ServerPrerendered", true);
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
            var pageTemplates = new List<PageTemplate>();

            pageTemplates.Add(new PageTemplate
            {
                Name = "Url Mappings",
                Parent = "Admin",
                Order = 33,
                Path = "admin/urlmappings",
                Icon = Icons.LinkBroken,
                IsNavigation = true,
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
            });

            pageTemplates.Add(new PageTemplate
            {
                Name = "Visitor Management",
                Parent = "Admin",
                Order = 35,
                Path = "admin/visitors",
                Icon = Icons.Eye,
                IsNavigation = true,
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
            });

            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
            foreach (Site site in sites.GetSites().ToList())
            {
                sites.CreatePages(site, pageTemplates);
            }
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
                        Content = "<p>The page you requested does not exist.</p>"
                    }
                }
            });

            var pages = scope.ServiceProvider.GetRequiredService<IPageRepository>();

            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
            foreach (Site site in sites.GetSites().ToList())
            {
                if (!pages.GetPages(site.SiteId).ToList().Where(item => item.Path == "404").Any())
                {
                    sites.CreatePages(site, pageTemplates);
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
                Debug.WriteLine($"Oqtane Error: Error In 3.2.0 Upgrade Logic - {ex}");
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
                Debug.WriteLine($"Oqtane Error: Error In 3.2.1 Upgrade Logic - {ex}");
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
                Debug.WriteLine($"Oqtane Error: Error In 3.3.0 Upgrade Logic - {ex}");
            }
        }
    }
}
