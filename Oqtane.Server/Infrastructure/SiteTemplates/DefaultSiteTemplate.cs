using Oqtane.Models;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using Oqtane.Repository;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Extensions;
using Oqtane.Shared;
using System.IO;

namespace Oqtane.SiteTemplates
{
    public class DefaultSiteTemplate : ISiteTemplate
    {

        private readonly IWebHostEnvironment _environment;
        private readonly ISiteRepository _siteRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;

        public DefaultSiteTemplate(IWebHostEnvironment environment, ISiteRepository siteRepository, IFolderRepository folderRepository, IFileRepository fileRepository)
        {
            _environment = environment;
            _siteRepository = siteRepository;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        public string Name
        {
            get { return "Default Site Template"; }
        }

        public List<PageTemplate> CreateSite(Site site)
        {
            List<PageTemplate> _pageTemplates = new List<PageTemplate>();

            _pageTemplates.Add(new PageTemplate
            {
                Name = "Home",
                Parent = "",
                Path = "",
                Icon = "home",
                IsNavigation = true,
                IsPersonalizable = false,
                EditMode = false,
                PagePermissions = new List<Permission> {
                    new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                    new Permission(PermissionNames.View, Constants.AdminRole, true),
                    new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                }.EncodePermissions() ,
                PageTemplateModules = new List<PageTemplateModule> {
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Welcome To Oqtane...", Pane = "Content", 
                        ModulePermissions = new List<Permission> {
                            new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                            new Permission(PermissionNames.View, Constants.AdminRole, true),
                            new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                        }.EncodePermissions(),
                        Content = "<p><a href=\"https://www.oqtane.org\" target=\"_new\">Oqtane</a> is an open source <b>modular application framework</b> built from the ground up using modern .NET Core technology. It leverages the revolutionary new Blazor component model to create a <b>fully dynamic</b> web development experience which can be executed on a client or server. Whether you are looking for a platform to <b>accelerate your web development</b> efforts, or simply interested in exploring the anatomy of a large-scale Blazor application, Oqtane provides a solid foundation based on proven enterprise architectural principles.</p>" +
                        "<p align=\"center\"><a href=\"https://www.oqtane.org\" target=\"_new\"><img class=\"img-fluid\" src=\"oqtane.png\"></a></p><p align=\"center\"><a class=\"btn btn-primary\" href=\"https://www.oqtane.org/Community\" target=\"_new\">Join Our Community</a>&nbsp;&nbsp;<a class=\"btn btn-primary\" href=\"https://github.com/oqtane/oqtane.framework\" target=\"_new\">Clone Our Repo</a></p>" +
                        "<p><a href=\"https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor\" target=\"_new\">Blazor</a> is a single-page app framework that lets you build interactive web applications using C# instead of JavaScript. Client-side Blazor relies on WebAssembly, an open web standard that does not require plugins or code transpilation in order to run natively in a web browser. Server-side Blazor uses SignalR to host your application on a web server and provide a responsive and robust debugging experience. Blazor applications works in all modern web browsers, including mobile browsers.</p>" +
                        "<p>Blazor is a feature of <a href=\"https://dotnet.microsoft.com/apps/aspnet\" target=\"_new\">ASP.NET Core 3</a>, the popular cross platform web development framework from Microsoft that extends the <a href=\"https://dotnet.microsoft.com/learn/dotnet/what-is-dotnet\" target=\"_new\" >.NET developer platform</a> with tools and libraries for building web apps.</p>"
                    },
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "MIT License", Pane = "Content",
                        ModulePermissions = new List<Permission> {
                            new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                            new Permission(PermissionNames.View, Constants.AdminRole, true),
                            new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                        }.EncodePermissions(),
                        Content = "<p>Copyright (c) 2019-2020 .NET Foundation</p>" +
                        "<p>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:</p>" +
                        "<p>The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.</p>" +
                        "<p>THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</p>"
                    },
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Secure Content", Pane = "Content",
                        ModulePermissions = new List<Permission> {
                            new Permission(PermissionNames.View, Constants.RegisteredRole, true),
                            new Permission(PermissionNames.View, Constants.AdminRole, true),
                            new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                        }.EncodePermissions(),
                        Content = "<p>Oqtane allows you to control access to your content using security roles. This module is only visible to Registered Users of the site.</p>"
                    }
                }
            });
            _pageTemplates.Add(new PageTemplate
            {
                Name = "Private",
                Parent = "",
                Path = "private",
                Icon = "lock-locked",
                IsNavigation = true,
                IsPersonalizable = false,
                EditMode = false,
                PagePermissions = new List<Permission> {
                    new Permission(PermissionNames.View, Constants.RegisteredRole, true),
                    new Permission(PermissionNames.View, Constants.AdminRole, true),
                    new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule> {
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "Secure Content", Pane = "Content",
                        ModulePermissions = new List<Permission> {
                            new Permission(PermissionNames.View, Constants.RegisteredRole, true),
                            new Permission(PermissionNames.View, Constants.AdminRole, true),
                            new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                        }.EncodePermissions(),
                        Content = "<p>Oqtane allows you to control access to your content using security roles. This page is only visible to Registered Users of the site.</p>"
                    }
                }
            });
            _pageTemplates.Add(new PageTemplate
            {
                Name = "My Page",
                Parent = "",
                Path = "mypage",
                Icon = "target",
                IsNavigation = true,
                IsPersonalizable = true,
                EditMode = false,
                PagePermissions = new List<Permission> {
                    new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                    new Permission(PermissionNames.View, Constants.AdminRole, true),
                    new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule> {
                    new PageTemplateModule { ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client", Title = "My Page", Pane = "Content",
                        ModulePermissions = new List<Permission> {
                            new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                            new Permission(PermissionNames.View, Constants.AdminRole, true),
                            new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                        }.EncodePermissions(),
                        Content = "<p>Oqtane offers native support for user personalized pages. If a page is identified as personalizable by the site administrator in the page settings, when an authenticated user visits the page they will see an edit button at the top right corner of the page next to their username. When they click this button the sytem will create a new version of the page and allow them to edit the page content.</p>"
                    }
                }
            });

            if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, "images", "logo.png")))
            {
                string folderpath = Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", site.TenantId.ToString(), "Sites", site.SiteId.ToString(),"\\");
                System.IO.Directory.CreateDirectory(folderpath);
                if (!System.IO.File.Exists(Path.Combine(folderpath, "logo.png")))
                {
                    System.IO.File.Copy(Path.Combine(_environment.WebRootPath, "images", "logo.png"), Path.Combine(folderpath, "logo.png"));
                }
                Folder folder = _folderRepository.GetFolder(site.SiteId, "");
                Oqtane.Models.File file = _fileRepository.AddFile(new Oqtane.Models.File { FolderId = folder.FolderId, Name = "logo.png", Extension = "png", Size = 8192, ImageHeight = 80, ImageWidth = 250 });
                site.LogoFileId = file.FileId;
                _siteRepository.UpdateSite(site);
            }

            return _pageTemplates;
        }
    }
}
