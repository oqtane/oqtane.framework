using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public class UpgradeManager : IUpgradeManager
    {
        private readonly IAliasRepository _aliases;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHostEnvironment _environment;

        public UpgradeManager(IAliasRepository aliases, IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment)
        {
            _aliases = aliases;
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
        }

        public void Upgrade(Tenant tenant, string version)
        {
            // core framework upgrade logic - executed for every tenant
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // set SiteState based on tenant
                var siteState = scope.ServiceProvider.GetRequiredService<SiteState>();
                siteState.Alias = new Alias { TenantId = tenant.TenantId };

                switch (version)
                {
                    case "1.0.0":
                        Upgrade_1_0_0(tenant, scope);
                        break;
                    case "2.0.2":
                        Upgrade_2_0_2(tenant, scope);
                        break;
                }
            }
        }

        private void Upgrade_1_0_0(Tenant tenant, IServiceScope scope)
        {
            var pageTemplates = new List<PageTemplate>();

            // **Note: this code is commented out on purpose - it provides an example of how to programmatically add a page to all existing sites on upgrade

            //pageTemplates.Add(new PageTemplate
            //{
            //    Name = "Test",
            //    Parent = "",
            //    Path = "test",
            //    Icon = Icons.Badge,
            //    IsNavigation = true,
            //    IsPersonalizable = false,
            //    EditMode = false,
            //    PagePermissions = new List<Permission>
            //    {
            //        new Permission(PermissionNames.View, RoleNames.Admin, true),
            //        new Permission(PermissionNames.View, RoleNames.Everyone, true),
            //        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
            //    }.EncodePermissions(),
            //            PageTemplateModules = new List<PageTemplateModule>
            //    {
            //        new PageTemplateModule
            //        {
            //            ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Login.Index).ToModuleDefinitionName(), Title = "Test", Pane = "Content",
            //            ModulePermissions = new List<Permission>
            //            {
            //                new Permission(PermissionNames.View, RoleNames.Admin, true),
            //                new Permission(PermissionNames.View, RoleNames.Everyone, true),
            //                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
            //            }.EncodePermissions(),
            //            Content = ""
            //        }
            //    }
            //});

            CreateSitePages(scope, pageTemplates);
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
                    catch
                    {
                        // error deleting directory
                    }
                }
            }

            // initialize SiteGuid
            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
            foreach (Site site in sites.GetSites().ToList())
            {
                site.SiteGuid = System.Guid.NewGuid().ToString();
                sites.UpdateSite(site);
            }
        }

        private void CreateSitePages(IServiceScope scope, List<PageTemplate> pageTemplates)
        {
            if (pageTemplates.Count != 0)
            {
                var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                foreach (Site site in sites.GetSites().ToList())
                {
                    sites.CreatePages(site, pageTemplates);
                }
            }
        }
    }
}
