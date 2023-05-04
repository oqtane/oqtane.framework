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

        public IEnumerable<Site> GetSites()
        {
            return _db.Site;
        }

        public Site AddSite(Site site)
        {
            site.SiteGuid = System.Guid.NewGuid().ToString();
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
            return GetSite(siteId, true);
        }

        public Site GetSite(int siteId, bool tracking)
        {
            if (tracking)
            {
                return _db.Site.Find(siteId);
            }
            else
            {
                return _db.Site.AsNoTracking().FirstOrDefault(item => item.SiteId == siteId);
            }
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
                {SiteId = site.SiteId, Name = "FirstName", Title = "First Name", Description = "Your First Or Given Name", Category = "Name", ViewOrder = 1, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = ""});
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "LastName", Title = "Last Name", Description = "Your Last Or Family Name", Category = "Name", ViewOrder = 2, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Street", Title = "Street", Description = "Street Or Building Address", Category = "Address", ViewOrder = 3, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(
                new Profile {SiteId = site.SiteId, Name = "City", Title = "City", Description = "City", Category = "Address", ViewOrder = 4, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Region", Title = "Region", Description = "State Or Province", Category = "Address", ViewOrder = 5, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Country", Title = "Country", Description = "Country", Category = "Address", ViewOrder = 6, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "PostalCode", Title = "Postal Code", Description = "Postal Code Or Zip Code", Category = "Address", ViewOrder = 7, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });
            _profileRepository.AddProfile(new Profile
                {SiteId = site.SiteId, Name = "Phone", Title = "Phone Number", Description = "Phone Number", Category = "Contact", ViewOrder = 8, MaxLength = 50, DefaultValue = "", IsRequired = false, IsPrivate = false, Options = "" });

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
                    Order = (pagetemplate.Order == 0) ? 1 : pagetemplate.Order,
                    Url = "",
                    IsNavigation = pagetemplate.IsNavigation,
                    ThemeType = "",
                    DefaultContainerType = "",
                    Icon = pagetemplate.Icon,
                    PermissionList = pagetemplate.PermissionList,
                    IsPersonalizable = pagetemplate.IsPersonalizable,
                    UserId = null,
                    IsClickable = true
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
                                PermissionList = pagetemplatemodule.PermissionList,
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
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Login.Index).ToModuleDefinitionName(), Title = "User Login", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
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
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Register.Index).ToModuleDefinitionName(), Title = "User Registration", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
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
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Reset.Index).ToModuleDefinitionName(), Title = "Password Reset", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
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
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.View, RoleNames.Registered, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.UserProfile.Index).ToModuleDefinitionName(), Title = "User Profile", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.View, RoleNames.Registered, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
                        Content = ""
                    }
                }
            });
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

            // admin pages
            pageTemplates.Add(new PageTemplate
            {
                Name = "Admin",
                Parent = "",
                Path = "admin",
                Icon = "",
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Dashboard.Index).ToModuleDefinitionName(), Title = "Admin Dashboard", Pane = PaneNames.Default,
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
                Name = "Site Settings",
                Parent = "Admin",
                Order = 1,
                Path = "admin/site",
                Icon = Icons.Home,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Site.Index).ToModuleDefinitionName(), Title = "Site Settings", Pane = PaneNames.Default,
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
                Name = "Page Management",
                Parent = "Admin",
                Order = 3,
                Path = "admin/pages",
                Icon = Icons.Layers,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Pages.Index).ToModuleDefinitionName(), Title = "Page Management", Pane = PaneNames.Default,
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
                Name = "User Management",
                Parent = "Admin",
                Order = 5,
                Path = "admin/users",
                Icon = Icons.People,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Users.Index).ToModuleDefinitionName(), Title = "User Management", Pane = PaneNames.Default,
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
                Name = "Profile Management",
                Parent = "Admin",
                Order = 7,
                Path = "admin/profiles",
                Icon = Icons.Person,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Profiles.Index).ToModuleDefinitionName(), Title = "Profile Management", Pane = PaneNames.Default,
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
                Name = "Role Management",
                Parent = "Admin",
                Order = 9,
                Path = "admin/roles",
                Icon = Icons.LockLocked,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Roles.Index).ToModuleDefinitionName(), Title = "Role Management", Pane = PaneNames.Default,
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
                Name = "File Management",
                Parent = "Admin",
                Order = 11,
                Path = "admin/files",
                Icon = Icons.File,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Files.Index).ToModuleDefinitionName(), Title = "File Management", Pane = PaneNames.Default,
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
                Name = "Recycle Bin",
                Parent = "Admin",
                Order = 13,
                Path = "admin/recyclebin",
                Icon = Icons.Trash,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.RecycleBin.Index).ToModuleDefinitionName(), Title = "Recycle Bin", Pane = PaneNames.Default,
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
                Name = "Url Mappings",
                Parent = "Admin",
                Order = 15,
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
                Order = 17,
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

            // host pages
            pageTemplates.Add(new PageTemplate
            {
                Name = "Event Log",
                Parent = "Admin",
                Order = 19,
                Path = "admin/log",
                Icon = Icons.MagnifyingGlass,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Logs.Index).ToModuleDefinitionName(), Title = "Event Log", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Site Management",
                Parent = "Admin",
                Order = 21,
                Path = "admin/sites",
                Icon = Icons.Globe,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Sites.Index).ToModuleDefinitionName(), Title = "Site Management", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Module Management",
                Parent = "Admin",
                Order = 23,
                Path = "admin/modules",
                Icon = Icons.Browser,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.ModuleDefinitions.Index).ToModuleDefinitionName(), Title = "Module Management", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Theme Management",
                Parent = "Admin",
                Order = 25,
                Path = "admin/themes",
                Icon = Icons.Brush,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Themes.Index).ToModuleDefinitionName(), Title = "Theme Management", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Language Management",
                Parent = "Admin",
                Order = 27,
                Path = "admin/languages",
                Icon = Icons.Text,
                IsNavigation = false,
                IsPersonalizable = false,
                PermissionList = new List<Permission>
                {
                    new Permission(PermissionNames.View, RoleNames.Host, true),
                    new Permission(PermissionNames.Edit, RoleNames.Host, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>
                {
                    new PageTemplateModule
                    {
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Languages.Index).ToModuleDefinitionName(), Title = "Language Management", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true),
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Scheduled Jobs",
                Parent = "Admin",
                Order = 29,
                Path = "admin/jobs",
                Icon = Icons.Timer,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Jobs.Index).ToModuleDefinitionName(), Title = "Scheduled Jobs", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "Sql Management",
                Parent = "Admin",
                Order = 31,
                Path = "admin/sql",
                Icon = Icons.Spreadsheet,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Sql.Index).ToModuleDefinitionName(), Title = "Sql Management", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "System Info",
                Parent = "Admin",
                Order = 33,
                Path = "admin/system",
                Icon = Icons.MedicalCross,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.SystemInfo.Index).ToModuleDefinitionName(), Title = "System Info", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });
            pageTemplates.Add(new PageTemplate
            {
                Name = "System Update",
                Parent = "Admin",
                Order = 35,
                Path = "admin/update",
                Icon = Icons.Aperture,
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
                        ModuleDefinitionName = typeof(Oqtane.Modules.Admin.Upgrade.Index).ToModuleDefinitionName(), Title = "System Update", Pane = PaneNames.Default,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Host, true),
                            new Permission(PermissionNames.Edit, RoleNames.Host, true)
                        },
                        Content = ""
                    }
                }
            });

            return pageTemplates;
        }
    }
}
