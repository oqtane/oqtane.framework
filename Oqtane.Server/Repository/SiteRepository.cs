using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Module = Oqtane.Models.Module;

namespace Oqtane.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private readonly TenantDBContext _db;
        private readonly IRoleRepository _roleRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly IPageModuleRepository _pageModuleRepository;
        private readonly IModuleDefinitionRepository _moduleDefinitionRepository;

        private readonly IServiceProvider _serviceProvider;

        private readonly IConfigurationRoot _config;

        public SiteRepository(TenantDBContext context, IRoleRepository roleRepository, IProfileRepository profileRepository, IFolderRepository folderRepository, IPageRepository pageRepository,
            IModuleRepository moduleRepository, IPageModuleRepository pageModuleRepository, IModuleDefinitionRepository moduleDefinitionRepository, IServiceProvider serviceProvider,
            IConfigurationRoot config)
        {
            _db = context;
            _roleRepository = roleRepository;
            _profileRepository = profileRepository;
            _folderRepository = folderRepository;
            _pageRepository = pageRepository;
            _moduleRepository = moduleRepository;
            _pageModuleRepository = pageModuleRepository;
            _moduleDefinitionRepository = moduleDefinitionRepository;
            _serviceProvider = serviceProvider;
            _config = config;
        }

        private List<PageTemplate> CreateAdminPages(List<PageTemplate> pageTemplates = null)
        {
            if (pageTemplates == null) pageTemplates = new List<PageTemplate>();

            // user pages
            pageTemplates.Add(new PageTemplate
            {
                Name = "Login",
                Parent = "",
                Path = "login",
                Icon = Icons.LockLocked,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Login.Index).ToModuleDefinitionName(), Title = "User Login", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Register",
                Parent = "",
                Path = "register",
                Icon = Icons.Person,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Register.Index).ToModuleDefinitionName(), Title = "User Registration", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });

            pageTemplates.Add(new PageTemplate
            {
                Name = "Reset",
                Parent = "",
                Path = "reset",
                Icon = Icons.Person,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Reset.Index).ToModuleDefinitionName(), Title = "Password Reset", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Profile",
                Parent = "",
                Path = "profile",
                Icon = Icons.Person,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Registered, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.UserProfile.Index).ToModuleDefinitionName(), Title = "User Profile", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Registered, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            
            // admin pages
            pageTemplates.Add(new PageTemplate
            {
                Name = "Admin", Parent = "", Path = "admin", Icon = "", IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Dashboard.Index).ToModuleDefinitionName(), Title = "Admin Dashboard", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Site Settings",
                Parent = "Admin",
                Path = "admin/site",
                Icon = Icons.Home,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Site.Index).ToModuleDefinitionName(), Title = "Site Settings", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Page Management",
                Parent = "Admin",
                Path = "admin/pages",
                Icon = Icons.Layers,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Pages.Index).ToModuleDefinitionName(), Title = "Page Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "User Management",
                Parent = "Admin",
                Path = "admin/users",
                Icon = Icons.People,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Users.Index).ToModuleDefinitionName(), Title = "User Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Profile Management",
                Parent = "Admin",
                Path = "admin/profiles",
                Icon = Icons.Person,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Profiles.Index).ToModuleDefinitionName(), Title = "Profile Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Role Management",
                Parent = "Admin",
                Path = "admin/roles",
                Icon = Icons.LockLocked,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Roles.Index).ToModuleDefinitionName(), Title = "Role Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "File Management",
                Parent = "Admin",
                Path = "admin/files",
                Icon = Icons.File,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Files.Index).ToModuleDefinitionName(), Title = "File Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Recycle Bin",
                Parent = "Admin",
                Path = "admin/recyclebin",
                Icon = Icons.Trash,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.RecycleBin.Index).ToModuleDefinitionName(), Title = "Recycle Bin", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });

            // host pages
            pageTemplates.Add(new PageTemplate
            {
                Name = "Event Log",
                Parent = "Admin",
                Path = "admin/log",
                Icon = Icons.MagnifyingGlass,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Logs.Index).ToModuleDefinitionName(), Title = "Event Log", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Site Management", Parent = "Admin", Path = "admin/sites", Icon = Icons.Globe, IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Sites.Index).ToModuleDefinitionName(), Title = "Site Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Module Management", Parent = "Admin", Path = "admin/modules", Icon = Icons.Browser, IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.ModuleDefinitions.Index).ToModuleDefinitionName(), Title = "Module Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Theme Management", Parent = "Admin", Path = "admin/themes", Icon = Icons.Brush, IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Themes.Index).ToModuleDefinitionName(), Title = "Theme Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Language Management",
                Parent = "Admin",
                Path = "admin/languages",
                Icon = Icons.Text,
                IsNavigation = false,
                IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Languages.Index).ToModuleDefinitionName(), Title = "Language Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true),
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Scheduled Jobs", Parent = "Admin", Path = "admin/jobs", Icon = Icons.Timer, IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Jobs.Index).ToModuleDefinitionName(), Title = "Scheduled Jobs", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Sql Management", Parent = "Admin", Path = "admin/sql", Icon = Icons.Spreadsheet, IsNavigation = false, IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Sql.Index).ToModuleDefinitionName(), Title = "Sql Management", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "System Info", Parent = "Admin", Path = "admin/system", Icon = Icons.MedicalCross, IsNavigation = false, IsPersonalizable = false,
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.SystemInfo.Index).ToModuleDefinitionName(), Title = "System Info", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "System Update", Parent = "Admin", Path = "admin/update", Icon = Icons.Aperture, IsNavigation = false, IsPersonalizable = false, 
                PagePermissions = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Upgrade.Index).ToModuleDefinitionName(), Title = "System Update", Pane = "Content",
                        ModulePermissions = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        }.EncodePermissions(),
                        Content = ""
                    }
                }
            });

            return pageTemplates;
        }

        public IEnumerable<Site> GetSites()
        {
            return _db.Site;
        }

        public Site AddSite(Site site)
        {
            
            _db.Site.Add(site);
            _db.SaveChanges();
            CreateSite(site);
            return site;
        }

        public Site UpdateSite(Site site)
        {
            _db.Entry(site).State = EntityState.Modified;
            _db.SaveChanges();
            return site;
        }

        public Site GetSite(int siteId)
        {
            return _db.Site.Find(siteId);
        }

        public void DeleteSite(int siteId)
        {
            var site = _db.Site.Find(siteId);
            _db.Site.Remove(site);
            _db.SaveChanges();
        }

        private void CreateSite(Site site)
        {
            // create default entities for site
            List<Role> roles = _roleRepository.GetRoles(site.SiteId, true).ToList();
            if (!roles.Where(item => item.Name == RoleNames.Everyone).Any())
            {
                _roleRepository.AddRole(new Role {SiteId = null, Name = RoleNames.Everyone, Description = "All Users", IsAutoAssigned = false, IsSystem = true});
            }

            if (!roles.Where(item => item.Name == RoleNames.Host).Any())
            {
                _roleRepository.AddRole(new Role {SiteId = null, Name = RoleNames.Host, Description = "Application Administrators", IsAutoAssigned = false, IsSystem = true});
            }

            _roleRepository.AddRole(new Role {SiteId = site.SiteId, Name = RoleNames.Registered, Description = "Registered Users", IsAutoAssigned = true, IsSystem = true});
            _roleRepository.AddRole(new Role {SiteId = site.SiteId, Name = RoleNames.Admin, Description = "Site Administrators", IsAutoAssigned = false, IsSystem = true});

            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "FirstName", Title = "First Name", Description = "Your First Or Given Name", Category = "Name", ViewOrder = 1, MaxLength = 50, DefaultValue = "", IsRequired = true, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "LastName", Title = "Last Name", Description = "Your Last Or Family Name", Category = "Name", ViewOrder = 2, MaxLength = 50, DefaultValue = "", IsRequired = true, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Street", Title = "Street", Description = "Street Or Building Address", Category = "Address", ViewOrder = 3, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});
            _profileRepository.AddProfile(
                new Profile {SiteId = site.SiteId, Name = "City", Title = "City", Description = "City", Category = "Address", ViewOrder = 4, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Region", Title = "Region", Description = "State Or Province", Category = "Address", ViewOrder = 5, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Country", Title = "Country", Description = "Country", Category = "Address", ViewOrder = 6, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "PostalCode", Title = "Postal Code", Description = "Postal Code Or Zip Code", Category = "Address", ViewOrder = 7, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Phone", Title = "Phone Number", Description = "Phone Number", Category = "Contact", ViewOrder = 8, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false});

            Folder folder = _folderRepository.AddFolder(new Folder
            {
                SiteId = site.SiteId, ParentId = null, Name = "Root", Path = "", Order = 1, IsSystem = true,
                Permissions = new List<Permission>
                {
                    new Permission(PermissionNames.Browse, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions()
            });
            _folderRepository.AddFolder(new Folder
            {
                SiteId = site.SiteId, ParentId = folder.FolderId, Name = "Users", Path = Utilities.PathCombine("Users",Path.DirectorySeparatorChar.ToString()), Order = 1, IsSystem = true,
                Permissions = new List<Permission>
                {
                    new Permission(PermissionNames.Browse, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                }.EncodePermissions()
            });

            // process site template first
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

            Type siteTemplateType = Type.GetType(site.SiteTemplateType);
            if (siteTemplateType != null)
            {
                var siteTemplateObject = ActivatorUtilities.CreateInstance(_serviceProvider, siteTemplateType);
                List<PageTemplate> pageTemplates = ((ISiteTemplate) siteTemplateObject).CreateSite(site);
                if (pageTemplates != null && pageTemplates.Count > 0)
                {
                    CreatePages(site, pageTemplates);
                }
            }

            // create admin pages
            CreatePages(site, CreateAdminPages());
        }

        public void CreatePages(Site site, List<PageTemplate> pageTemplates)
        {
            List<ModuleDefinition> moduledefinitions = _moduleDefinitionRepository.GetModuleDefinitions(site.SiteId).ToList();
            foreach (PageTemplate pagetemplate in pageTemplates)
            {
                int? parentid = null;
                if (pagetemplate.Parent != "")
                {
                    List<Page> pages = _pageRepository.GetPages(site.SiteId).ToList();
                    Page parent = pages.Where(item => item.Name == pagetemplate.Parent).FirstOrDefault();
                    parentid = parent.PageId;
                }

                Page page = new Page
                {
                    SiteId = site.SiteId,
                    ParentId = parentid,
                    Name = pagetemplate.Name,
                    Title = "",
                    Path = pagetemplate.Path,
                    Order = 1,
                    Url = "",
                    IsNavigation = pagetemplate.IsNavigation,
                    ThemeType = "",
                    LayoutType = "",
                    DefaultContainerType = "",
                    Icon = pagetemplate.Icon,
                    Permissions = pagetemplate.PagePermissions,
                    IsPersonalizable = pagetemplate.IsPersonalizable,
                    UserId = null
                };
                page = _pageRepository.AddPage(page);

                foreach (PageTemplateModule pagetemplatemodule in pagetemplate.PageTemplateModules)
                {
                    if (pagetemplatemodule.ModuleDefinitionName != "")
                    {
                        ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == pagetemplatemodule.ModuleDefinitionName).FirstOrDefault();
                        if (moduledefinition != null)
                        {
                            Module module = new Module
                            {
                                SiteId = site.SiteId,
                                ModuleDefinitionName = pagetemplatemodule.ModuleDefinitionName,
                                AllPages = false,
                                Permissions = pagetemplatemodule.ModulePermissions,
                            };
                            module = _moduleRepository.AddModule(module);

                            if (pagetemplatemodule.Content != "" && moduledefinition.ServerManagerType!= "")
                            {
                                Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                                if (moduletype != null && moduletype.GetInterface("IPortable") != null)
                                {
                                    try
                                    {
                                        var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                        ((IPortable)moduleobject).ImportModule(module, pagetemplatemodule.Content, moduledefinition.Version);
                                    }
                                    catch
                                    {
                                        // error in IPortable implementation
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
                            _pageModuleRepository.AddPageModule(pagemodule);
                        }
                    }
                }
            }
        }
    }
}
