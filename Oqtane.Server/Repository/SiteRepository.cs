using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Reflection;
using Oqtane.Modules;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IModuleDefinitionRepository ModuleDefinitionRepository;
        private readonly IServiceProvider ServiceProvider;
        private readonly List<PageTemplate> SiteTemplate;

        public SiteRepository(TenantDBContext context, IRoleRepository RoleRepository, IProfileRepository ProfileRepository, IPageRepository PageRepository, IModuleRepository ModuleRepository, IPageModuleRepository PageModuleRepository, IModuleDefinitionRepository ModuleDefinitionRepository, IServiceProvider ServiceProvider)
        {
            db = context;
            this.RoleRepository = RoleRepository;
            this.ProfileRepository = ProfileRepository;
            this.PageRepository = PageRepository;
            this.ModuleRepository = ModuleRepository;
            this.PageModuleRepository = PageModuleRepository;
            this.ModuleDefinitionRepository = ModuleDefinitionRepository;
            this.ServiceProvider = ServiceProvider;

            // define the default site template
            SiteTemplate = new List<PageTemplate>();
            SiteTemplate.Add(new PageTemplate { Name = "Home", Parent = "", Path = "", Icon = "home", IsNavigation = true, IsPersonalizable = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Welcome To Oqtane...", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]",  
                    Content = "<p><a href=\"https://www.oqtane.org\" target=\"_new\">Oqtane</a> is an open source <b>modular application framework</b> built from the ground up using modern .NET Core technology. It leverages the revolutionary new Blazor component model to create a <b>fully dynamic</b> web development experience which can be executed on a client or server. Whether you are looking for a platform to <b>accelerate your web development</b> efforts, or simply interested in exploring the anatomy of a large-scale Blazor application, Oqtane provides a solid foundation based on proven enterprise architectural principles.</p>" +
                    "<p align=\"center\"><a href=\"https://www.oqtane.org\" target=\"_new\"><img src=\"oqtane.png\"></a><br /><br /><a class=\"btn btn-primary\" href=\"https://www.oqtane.org/Community\" target=\"_new\">Join Our Community</a>&nbsp;&nbsp;<a class=\"btn btn-primary\" href=\"https://github.com/oqtane/oqtane.framework\" target=\"_new\">Clone Our Repo</a><br /><br /></p>" +
                    "<p><a href=\"https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor\" target=\"_new\">Blazor</a> is a single-page app framework that lets you build interactive web applications using C# instead of JavaScript. Client-side Blazor relies on WebAssembly, an open web standard that does not require plugins or code transpilation in order to run natively in a web browser. Server-side Blazor uses SignalR to host your application on a web server and provide a responsive and robust debugging experience. Blazor applications works in all modern web browsers, including mobile browsers.</p>" +
                    "<p>Blazor is a feature of <a href=\"https://dotnet.microsoft.com/apps/aspnet\" target=\"_new\">ASP.NET Core 3.0</a>, the popular cross platform web development framework from Microsoft that extends the <a href=\"https://dotnet.microsoft.com/learn/dotnet/what-is-dotnet\" target=\"_new\" >.NET developer platform</a> with tools and libraries for building web apps.</p>"
                },
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "MIT License", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]",  
                    Content = "<p>Copyright (c) 2019 .NET Foundation</p>" +
                    "<p>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:</p>" +
                    "<p>The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.</p>" +
                    "<p>THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</p>"
                }
            }
            }); 
            SiteTemplate.Add(new PageTemplate { Name = "My Page", Parent = "", Path = "portal", Icon = "target", IsNavigation = true, IsPersonalizable = true, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "My Page", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]",  
                    Content = "<p>Oqtane offers native support for user personalized pages. If a page is identified as personalizable by the site administrator in the page settings, when an authenticated user visits the page they will see an edit button at the top right corner of the page next to their username. When they click this button the sytem will create a new version of the page and allow them to edit the page content.</p>"
                }
            }
            }); 
            SiteTemplate.Add(new PageTemplate { Name = "Admin", Parent = "", Path = "admin", Icon = "", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Dashboard, Oqtane.Client", Title = "Admin Dashboard", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Site Management", Parent = "Admin", Path = "admin/sites", Icon = "globe", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Sites, Oqtane.Client", Title = "Site Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Page Management", Parent = "Admin", Path = "admin/pages", Icon = "layers", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Pages, Oqtane.Client", Title = "Page Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "User Management", Parent = "Admin", Path = "admin/users", Icon = "people", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Users, Oqtane.Client", Title = "User Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Profile Management", Parent = "Admin", Path = "admin/profiles", Icon = "person", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Profiles, Oqtane.Client", Title = "Profile Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Role Management", Parent = "Admin", Path = "admin/roles", Icon = "lock-locked", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Roles, Oqtane.Client", Title = "Role Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Event Log", Parent = "Admin", Path = "admin/log", Icon = "magnifying-glass", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Logs, Oqtane.Client", Title = "Event Log", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "File Management", Parent = "Admin", Path = "admin/files", Icon = "file", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Files, Oqtane.Client", Title = "File Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Recycle Bin", Parent = "Admin", Path = "admin/recyclebin", Icon = "trash", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.RecycleBin, Oqtane.Client", Title = "Recycle Bin", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Tenant Management", Parent = "Admin", Path = "admin/tenants", Icon = "list", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Tenants, Oqtane.Client", Title = "Tenant Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Module Management", Parent = "Admin", Path = "admin/modules", Icon = "browser", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.ModuleDefinitions, Oqtane.Client", Title = "Module Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Theme Management", Parent = "Admin", Path = "admin/themes", Icon = "brush", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Themes, Oqtane.Client", Title = "Theme Management", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Scheduled Jobs", Parent = "Admin", Path = "admin/jobs", Icon = "timer", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Jobs, Oqtane.Client", Title = "Scheduled Jobs", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Upgrade Service", Parent = "Admin", Path = "admin/upgrade", Icon = "aperture", IsNavigation = false, IsPersonalizable = false, EditMode = true, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Upgrade, Oqtane.Client", Title = "Upgrade Service", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Login", Parent = "", Path = "login", Icon = "lock-locked", IsNavigation = false, IsPersonalizable = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Login, Oqtane.Client", Title = "User Login", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Register", Parent = "", Path = "register", Icon = "person", IsNavigation = false, IsPersonalizable = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.Register, Oqtane.Client", Title = "User Registration", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
            SiteTemplate.Add(new PageTemplate { Name = "Profile", Parent = "", Path = "profile", Icon = "person", IsNavigation = false, IsPersonalizable = false, EditMode = false, PagePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", PageTemplateModules = new List<PageTemplateModule> {
                new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.Admin.UserProfile, Oqtane.Client", Title = "User Profile", Pane = "Content", ModulePermissions = "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]", Content = "" }
            }});
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

            List<ModuleDefinition> moduledefinitions = ModuleDefinitionRepository.GetModuleDefinitions(site.SiteId).ToList();
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
                    Order = 1,
                    IsNavigation = pagetemplate.IsNavigation,
                    EditMode = pagetemplate.EditMode,
                    ThemeType = "",
                    LayoutType = "",
                    Icon = pagetemplate.Icon,
                    Permissions = pagetemplate.PagePermissions,
                    IsPersonalizable = pagetemplate.IsPersonalizable,
                    UserId = null
                };
                page = PageRepository.AddPage(page);

                foreach(PageTemplateModule pagetemplatemodule in pagetemplate.PageTemplateModules)
                {
                    if (pagetemplatemodule.ModuleDefinitionName != "")
                    {
                        ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == pagetemplatemodule.ModuleDefinitionName).FirstOrDefault();
                        if (moduledefinition != null)
                        {
                            Models.Module module = new Models.Module
                            {
                                SiteId = site.SiteId,
                                ModuleDefinitionName = pagetemplatemodule.ModuleDefinitionName,
                                Permissions = pagetemplatemodule.ModulePermissions,
                            };
                            module = ModuleRepository.AddModule(module);

                            if (pagetemplatemodule.Content != "" && moduledefinition.ServerAssemblyName != "")
                            {
                                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(item => item.FullName.StartsWith(moduledefinition.ServerAssemblyName)).FirstOrDefault();
                                if (assembly != null)
                                {
                                    Type moduletype = assembly.GetTypes()
                                        .Where(item => item.Namespace != null)
                                        .Where(item => item.Namespace.StartsWith(moduledefinition.ModuleDefinitionName.Substring(0, moduledefinition.ModuleDefinitionName.IndexOf(","))))
                                        .Where(item => item.GetInterfaces().Contains(typeof(IPortable))).FirstOrDefault();
                                    if (moduletype != null)
                                    {
                                        var moduleobject = ActivatorUtilities.CreateInstance(ServiceProvider, moduletype);
                                        ((IPortable)moduleobject).ImportModule(module, pagetemplatemodule.Content, moduledefinition.Version);
                                    }
                                }
                            }

                            PageModule pagemodule = new PageModule
                            {
                                PageId = page.PageId,
                                ModuleId = module.ModuleId,
                                Title = pagetemplatemodule.Title,
                                Pane = pagetemplatemodule.Pane,
                                Order = 1,
                                ContainerType = ""
                            };
                            PageModuleRepository.AddPageModule(pagemodule);
                        }

                    }
                }
            }
        }
    }
}
