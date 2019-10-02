using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Oqtane.Shared;
using System;

namespace Oqtane.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private readonly TenantDBContext db;
        private readonly IRoleRepository RoleRepository;
        private readonly IProfileRepository ProfileRepository;
        private readonly IPageRepository PageRepository;
        private readonly IModuleRepository ModuleRepository;
        private readonly IPageModuleRepository PageModuleRepository;
        private readonly List<PageTemplate> SiteTemplate;

        public SiteRepository(TenantDBContext context, IRoleRepository RoleRepository, IProfileRepository ProfileRepository, IPageRepository PageRepository, IModuleRepository ModuleRepository, IPageModuleRepository PageModuleRepository)
        {
            db = context;
            this.RoleRepository = RoleRepository;
            this.ProfileRepository = ProfileRepository;
            this.PageRepository = PageRepository;
            this.ModuleRepository = ModuleRepository;
            this.PageModuleRepository = PageModuleRepository;

            // defines the default site template
            SiteTemplate = new List<PageTemplate>();
            SiteTemplate.Add(new PageTemplate { Name = "Home", Parent = "", Path = "", Order = 1, Icon = "home", IsNavigation = true, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "", ModulePermissions = "", Title = "", Pane = "", ContainerType = "" });
            SiteTemplate.Add(new PageTemplate { Name = "Admin", Parent = "", Path = "admin", Order = 1, Icon = "", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Dashboard, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Administration", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Site Management", Parent = "Admin", Path = "admin/sites", Order = 1, Icon = "globe", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Sites, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Site Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Page Management", Parent = "Admin", Path = "admin/pages", Order = 1, Icon = "layers", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Pages, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Page Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Module Management", Parent = "Admin", Path = "admin/modules", Order = 1, Icon = "browser", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.ModuleDefinitions, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Module Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Theme Management", Parent = "Admin", Path = "admin/themes", Order = 1, Icon = "brush", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Themes, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Theme Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "User Management", Parent = "Admin", Path = "admin/users", Order = 1, Icon = "person", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Users, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "User Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Role Management", Parent = "Admin", Path = "admin/roles", Order = 1, Icon = "lock-locked", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Roles, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Role Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Tenant Management", Parent = "Admin", Path = "admin/tenants", Order = 1, Icon = "list", IsNavigation = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Tenants, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Site Management", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Login", Parent = "", Path = "login", Order = 1, Icon = "lock-locked", IsNavigation = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Login, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Login", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Register", Parent = "", Path = "register", Order = 1, Icon = "person", IsNavigation = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Register, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "Register", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
            SiteTemplate.Add(new PageTemplate { Name = "Profile", Parent = "", Path = "profile", Order = 1, Icon = "person", IsNavigation = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", 
                ModuleDefinitionName = "Oqtane.Modules.Admin.Profile, Oqtane.Client", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Title = "User Profile", Pane = "top", ContainerType = "Oqtane.Themes.Theme2.Container2, Oqtane.Client" });
        }

        public IEnumerable<Site> GetSites()
        {
            return db.Site;
        }

        public Site AddSite(Site Site)
        {
            db.Site.Add(Site);
            db.SaveChanges();
            CreateSite(Site);
            return Site;
        }

        public Site UpdateSite(Site Site)
        {
            db.Entry(Site).State = EntityState.Modified;
            db.SaveChanges();
            return Site;
        }

        public Site GetSite(int siteId)
        {
            return db.Site.Find(siteId);
        }

        public void DeleteSite(int siteId)
        {
            Site site = db.Site.Find(siteId);
            db.Site.Remove(site);
            db.SaveChanges();
        }

        private void CreateSite(Site site)
        {
            List<Role> roles = RoleRepository.GetRoles(site.SiteId, true).ToList();
            if (!roles.Where(item => item.Name == Constants.AllUsersRole).Any())
            {
                RoleRepository.AddRole(new Role { SiteId = null, Name = Constants.AllUsersRole, Description = "All Users", IsAutoAssigned = false, IsSystem = true });
            }
            if (!roles.Where(item => item.Name == Constants.HostRole).Any())
            {
                RoleRepository.AddRole(new Role { SiteId = null, Name = Constants.HostRole, Description = "Application Administrators", IsAutoAssigned = false, IsSystem = true });
            }

            RoleRepository.AddRole(new Role { SiteId = site.SiteId, Name = Constants.RegisteredRole, Description = "Registered Users", IsAutoAssigned = true, IsSystem = true });
            RoleRepository.AddRole(new Role { SiteId = site.SiteId, Name = Constants.AdminRole, Description = "Site Administrators", IsAutoAssigned = false, IsSystem = true });

            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "FirstName", Title = "First Name", Description = "Your First Or Given Name", Category = "Name", ViewOrder = 1, MaxLength = 50, DefaultValue = "", IsRequired = true, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "LastName", Title = "Last Name", Description = "Your Last Or Family Name", Category = "Name", ViewOrder =  2, MaxLength = 50, DefaultValue = "", IsRequired = true, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "Street", Title = "Street", Description = "Street Or Building Address", Category = "Address", ViewOrder = 3, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "City", Title = "City", Description = "City", Category = "Address", ViewOrder = 4, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false }); 
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "Region", Title = "Region", Description = "State Or Province", Category = "Address", ViewOrder = 5, MaxLength = 50, DefaultValue = "", IsRequired  = false, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "Country", Title = "Country", Description = "Country", Category = "Address", ViewOrder = 6, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "PostalCode", Title = "Postal Code", Description = "Postal Code Or Zip Code", Category = "Address", ViewOrder = 7, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false });
            ProfileRepository.AddProfile(new Profile { SiteId = site.SiteId, Name = "Phone", Title = "Phone Number", Description = "Phone Number", Category = "Contact", ViewOrder = 8, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false });

            foreach (PageTemplate pagetemplate in SiteTemplate)
            {
                int? parentid = null;
                if (pagetemplate.Parent != "")
                {
                    List<Page> pages = PageRepository.GetPages(site.SiteId).ToList();
                    Page parent = pages.Where(item => item.Name == pagetemplate.Parent).FirstOrDefault();
                    parentid = parent.PageId;
                }

                Page page = new Page
                {
                    SiteId = site.SiteId,
                    ParentId = parentid,
                    Name = pagetemplate.Name,
                    Path = pagetemplate.Path,
                    Order = pagetemplate.Order,
                    IsNavigation = pagetemplate.IsNavigation,
                    EditMode = pagetemplate.EditMode,
                    ThemeType = site.DefaultThemeType,
                    LayoutType = site.DefaultLayoutType,
                    Icon = pagetemplate.Icon,
                    Permissions = pagetemplate.PagePermissions
                };
                Type type = Type.GetType(page.ThemeType);
                System.Reflection.PropertyInfo property = type.GetProperty("Panes");
                page.Panes = (string)property.GetValue(Activator.CreateInstance(type), null);
                page = PageRepository.AddPage(page);

                if (pagetemplate.ModuleDefinitionName != "")
                {
                    Module module = new Module
                    {
                        SiteId = site.SiteId,
                        ModuleDefinitionName = pagetemplate.ModuleDefinitionName,
                        Permissions = pagetemplate.ModulePermissions,
                    };
                    module = ModuleRepository.AddModule(module);

                    PageModule pagemodule = new PageModule
                    {
                        PageId = page.PageId,
                        ModuleId = module.ModuleId,
                        Title = pagetemplate.Title,
                        Pane = pagetemplate.Pane,
                        Order = 1,
                        ContainerType = pagetemplate.ContainerType
                    };
                    PageModuleRepository.AddPageModule(pagemodule);
                }
            }
        }
    }
}
