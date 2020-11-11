using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Infrastructure
{
    public class UpgradeManager : IUpgradeManager
    {
        private readonly IAliasRepository _aliases;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpgradeManager(IAliasRepository aliases, IServiceScopeFactory serviceScopeFactory)
        {
            _aliases = aliases;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Upgrade(Tenant tenant, string version)
        {
            // core framework upgrade logic - note that you can check if current tenant is Master if you only want to execute logic once
            var pageTemplates = new List<PageTemplate>();

            switch (version)
            {
                case "0.9.0":
                    // add a page to all existing sites on upgrade

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
                    CreateSitePages(tenant, pageTemplates);
                    break;
            }
        }

        private void CreateSitePages(Tenant tenant, List<PageTemplate> pageTemplates)
        {
            if (pageTemplates.Count != 0)
            {
                var processed = new List<Site>();
                foreach (Alias alias in _aliases.GetAliases().Where(item => item.TenantId == tenant.TenantId))
                {
                    if (!processed.Exists(item => item.SiteId == alias.SiteId))
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var siteState = scope.ServiceProvider.GetRequiredService<SiteState>();
                            siteState.Alias = alias;
                            var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                            var site = sites.GetSite(alias.SiteId);
                            if (site != null)
                            {
                                sites.CreatePages(site, pageTemplates);
                            }
                            processed.Add(site);
                        }
                    }
                }
            }
        }
    }
}
