using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class SynchronizationJob : HostedServiceBase
    {
        // JobType = "Oqtane.Infrastructure.SynchronizationJob, Oqtane.Server"

        // synchronization only supports sites in the same tenant (database)
        // module title is used as a key to identify module instances on a page (ie. using "-" as a module title is problematic ie. content as configuration)
        // relies on Module.ModifiedOn to be set if the module content changes (for efficiency)
        // modules must implement ISynchronizable interface (new interface as IPortable was generally only implemented in an additive manner)

        // define settings that should not be synchronized (should be extensible in the future)
        List<Setting> excludedSettings = new List<Setting>() {
            new Setting { EntityName = EntityNames.Site, SettingName = "Search_LastIndexedOn" }
        };

        public SynchronizationJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Synchronization Job";
            Frequency = "m"; // minute
            Interval = 1;
            IsEnabled = false;
        }

        // job is executed for each tenant in installation
        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            var siteGroupDefinitionRepository = provider.GetRequiredService<ISiteGroupDefinitionRepository>();
            var siteGroupRepository = provider.GetRequiredService<ISiteGroupRepository>();
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var aliasRepository = provider.GetRequiredService<IAliasRepository>();
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();

            List<SiteGroup> siteGroups = null;
            List<Site> sites = null;
            List<Alias> aliases = null;

            // get groups
            var groups = siteGroupDefinitionRepository.GetSiteGroupDefinitions();

            // iterate through groups which need to be synchronized
            foreach (var group in groups.Where(item => item.Synchronization && item.Synchronize))
            {
                // get data
                if (siteGroups == null)
                {
                    siteGroups = siteGroupRepository.GetSiteGroups().ToList();
                    sites = siteRepository.GetSites().ToList();
                    aliases = aliasRepository.GetAliases().ToList();
                }

                var aliasName = "https://" + aliases.First(item => item.TenantId == tenantManager.GetTenant().TenantId && item.SiteId == group.PrimarySiteId && item.IsDefault).Name;
                log += $"Processing Primary Site: {sites.First(item => item.SiteId == group.PrimarySiteId).Name} - {CreateLink(aliasName)}<br />";

                // get primary site
                var primarySite = sites.FirstOrDefault(item => item.SiteId == group.PrimarySiteId);
                if (primarySite != null)
                {
                    // update flag to prevent job from processing group again
                    group.Synchronize = false;
                    siteGroupDefinitionRepository.UpdateSiteGroupDefinition(group);

                    // iterate through sites in group
                    foreach (var siteGroup in siteGroups.Where(item => item.SiteGroupDefinitionId == group.SiteGroupDefinitionId && item.SiteId != group.PrimarySiteId))
                    {
                        // get secondary site
                        var secondarySite = sites.FirstOrDefault(item => item.SiteId == siteGroup.SiteId);
                        if (secondarySite != null)
                        {
                            // get default alias for site
                            siteGroup.AliasName = "https://" + aliases.First(item => item.TenantId == tenantManager.GetTenant().TenantId && item.SiteId == siteGroup.SiteId && item.IsDefault).Name;

                            // initialize SynchronizedOn
                            if (siteGroup.SynchronizedOn == null)
                            {
                                siteGroup.SynchronizedOn = DateTime.MinValue;
                            }

                            // replicate site
                            var siteLog = ReplicateSite(provider, tenantManager, settingRepository, siteGroup, primarySite, secondarySite);

                            // set synchronized on date/time
                            siteGroup.SynchronizedOn = DateTime.UtcNow;
                            siteGroupRepository.UpdateSiteGroup(siteGroup);

                            log += $"Processed Secondary Site: {secondarySite.Name} - {CreateLink(siteGroup.AliasName)}<br />" + siteLog;
                        }
                        else
                        {
                            log += $"Site Group {group.Name} Has A SiteId {siteGroup.SiteId} Which Does Not Exist<br />";
                        }
                    }
                }
                else
                {
                    log += $"Site Group {group.Name} Has A PrimarySiteId {group.PrimarySiteId} Which Does Not Exist<br />";
                }
            }

            if (string.IsNullOrEmpty(log))
            {
                log = "No Site Groups Require Synchronization<br />";
            }

            return log;
        }

        private string ReplicateSite(IServiceProvider provider, ITenantManager tenantManager, ISettingRepository settingRepository, SiteGroup siteGroup, Site primarySite, Site secondarySite)
        {
            var log = "";

            // replicate roles/users
            log += ReplicateRoles(provider, settingRepository, siteGroup, primarySite.SiteId, secondarySite.SiteId);

            // replicate folders/files
            log += ReplicateFolders(provider, settingRepository, siteGroup, primarySite.SiteId, secondarySite.SiteId);

            // replicate pages/modules
            log += ReplicatePages(provider, settingRepository, tenantManager, siteGroup, primarySite.SiteId, secondarySite.SiteId);

            // replicate site
            if (primarySite.ModifiedOn > siteGroup.SynchronizedOn)
            {
                secondarySite.TimeZoneId = primarySite.TimeZoneId;
                if (secondarySite.LogoFileId != primarySite.LogoFileId)
                {
                    secondarySite.LogoFileId = ResolveFileId(provider, primarySite.LogoFileId, secondarySite.SiteId);
                }
                if (secondarySite.FaviconFileId != primarySite.FaviconFileId)
                {
                    secondarySite.FaviconFileId = ResolveFileId(provider, primarySite.FaviconFileId, secondarySite.SiteId); ;
                }
                secondarySite.DefaultThemeType = primarySite.DefaultThemeType;
                secondarySite.DefaultContainerType = primarySite.DefaultContainerType;
                secondarySite.AdminContainerType = primarySite.AdminContainerType;
                secondarySite.PwaIsEnabled = primarySite.PwaIsEnabled;
                if (secondarySite.PwaAppIconFileId != primarySite.PwaAppIconFileId)
                {
                    secondarySite.PwaAppIconFileId = ResolveFileId(provider, primarySite.PwaAppIconFileId, secondarySite.SiteId); ;
                }
                if (secondarySite.PwaSplashIconFileId != primarySite.PwaSplashIconFileId)
                {
                    secondarySite.PwaSplashIconFileId = ResolveFileId(provider, primarySite.PwaSplashIconFileId, secondarySite.SiteId); ;
                }
                secondarySite.AllowRegistration = primarySite.AllowRegistration;
                secondarySite.VisitorTracking = primarySite.VisitorTracking;
                secondarySite.CaptureBrokenUrls = primarySite.CaptureBrokenUrls;
                secondarySite.SiteGuid = primarySite.SiteGuid;
                secondarySite.RenderMode = primarySite.RenderMode;
                secondarySite.Runtime = primarySite.Runtime;
                secondarySite.Prerender = primarySite.Prerender;
                secondarySite.Hybrid = primarySite.Hybrid;
                secondarySite.EnhancedNavigation = primarySite.EnhancedNavigation;
                secondarySite.Version = primarySite.Version;
                secondarySite.HeadContent = primarySite.HeadContent;
                secondarySite.BodyContent = primarySite.BodyContent;
                secondarySite.CreatedBy = primarySite.CreatedBy;
                secondarySite.CreatedOn = primarySite.CreatedOn;
                secondarySite.ModifiedBy = primarySite.ModifiedBy;
                secondarySite.ModifiedOn = primarySite.ModifiedOn;
                secondarySite.IsDeleted = primarySite.IsDeleted;
                secondarySite.DeletedBy = primarySite.DeletedBy;
                secondarySite.DeletedOn = primarySite.DeletedOn;

                var siteRepository = provider.GetRequiredService<ISiteRepository>();
                if (siteGroup.Synchronize)
                {
                    siteRepository.UpdateSite(secondarySite);
                }
                log += Log(siteGroup, $"Secondary Site Updated: {secondarySite.Name}");
            }

            // site settings
            log += ReplicateSettings(settingRepository, siteGroup, EntityNames.Site, primarySite.SiteId, secondarySite.SiteId);

            if (siteGroup.SynchronizedOn == DateTime.MinValue || !string.IsNullOrEmpty(log))
            {
                // clear cache for secondary site if any content was replicated
                var syncManager = provider.GetRequiredService<ISyncManager>();
                var alias = new Alias { TenantId = tenantManager.GetTenant().TenantId, SiteId = secondarySite.SiteId }; 
                syncManager.AddSyncEvent(alias, EntityNames.Site, secondarySite.SiteId, SyncEventActions.Refresh);
            }

            if (!string.IsNullOrEmpty(log) && siteGroup.Notify)
            {
                // send change log to administrators
                SendNotifications(provider, secondarySite.SiteId, secondarySite.Name, log);
            }

            return log;
        }

        private int? ResolveFileId(IServiceProvider provider, int? fileId, int siteId)
        {
            if (fileId != null)
            {
                var fileRepository = provider.GetRequiredService<IFileRepository>();
                var file = fileRepository.GetFile(fileId.Value);
                fileId = fileRepository.GetFile(siteId, file.Folder.Path, file.Name).FileId;
            }
            return fileId;
        }

        private string ReplicateRoles(IServiceProvider provider, ISettingRepository settingRepository, SiteGroup siteGroup, int primarySiteId, int secondarySiteId)
        {
            // get roles
            var roleRepository = provider.GetRequiredService<IRoleRepository>();
            var primaryRoles = roleRepository.GetRoles(primarySiteId);
            var secondaryRoles = roleRepository.GetRoles(secondarySiteId).ToList();
            var log = "";

            foreach (var primaryRole in primaryRoles)
            {
                var role = secondaryRoles.FirstOrDefault(item => item.Name == primaryRole.Name);

                var secondaryRole = role;
                if (secondaryRole == null)
                {
                    secondaryRole = new Role();
                    secondaryRole.SiteId = secondarySiteId;
                }

                if (role == null || primaryRole.ModifiedOn > siteGroup.SynchronizedOn)
                {
                    // set all properties
                    secondaryRole.Name = primaryRole.Name;
                    secondaryRole.Description = primaryRole.Description;
                    secondaryRole.IsAutoAssigned = primaryRole.IsAutoAssigned;
                    secondaryRole.IsSystem = primaryRole.IsSystem;

                    if (role == null)
                    {
                        if (siteGroup.Synchronize)
                        {
                            roleRepository.AddRole(secondaryRole);
                        }
                        log += Log(siteGroup, $"Role Added: {secondaryRole.Name}");
                    }
                    else
                    {
                        if (siteGroup.Synchronize)
                        {
                            roleRepository.UpdateRole(secondaryRole);
                        }
                        log += Log(siteGroup, $"Role Updated: {secondaryRole.Name}");
                        secondaryRoles.Remove(role);
                    }
                }
            }

            // remove roles in the secondary site which do not exist in the primary site
            foreach (var secondaryRole in secondaryRoles.Where(item => !primaryRoles.Select(item => item.Name).Contains(item.Name)))
            {
                if (siteGroup.Synchronize)
                {
                    roleRepository.DeleteRole(secondaryRole.RoleId);
                }
                log += Log(siteGroup, $"Role Deleted: {secondaryRole.Name}");
            }

            // settings
            log += ReplicateSettings(settingRepository, siteGroup, EntityNames.Role, primarySiteId, secondarySiteId);

            return log;
        }

        private string ReplicateFolders(IServiceProvider provider, ISettingRepository settingRepository, SiteGroup siteGroup, int primarySiteId, int secondarySiteId)
        {
            var folderRepository = provider.GetRequiredService<IFolderRepository>();
            var fileRepository = provider.GetRequiredService<IFileRepository>();
            var log = "";

            // get folders (ignore personalized)
            var primaryFolders = folderRepository.GetFolders(primarySiteId).Where(item => !item.Path.StartsWith("Users/"));
            var secondaryFolders = folderRepository.GetFolders(secondarySiteId).Where(item => !item.Path.StartsWith("Users/")).ToList();

            // iterate through folders 
            foreach (var primaryFolder in primaryFolders)
            {
                var folder = secondaryFolders.FirstOrDefault(item => item.Path == primaryFolder.Path);

                var secondaryFolder = folder;
                if (secondaryFolder == null)
                {
                    secondaryFolder = new Folder();
                    secondaryFolder.SiteId = secondarySiteId;
                }

                if (folder == null || primaryFolder.ModifiedOn > siteGroup.SynchronizedOn)
                {
                    // set all properties
                    secondaryFolder.ParentId = null;
                    if (primaryFolder.ParentId != null)
                    {
                        var parentFolder = folderRepository.GetFolder(secondarySiteId, primaryFolders.First(item => item.FolderId == primaryFolder.ParentId).Path);
                        if (parentFolder != null)
                        {
                            secondaryFolder.ParentId = parentFolder.FolderId;
                        }
                    }
                    secondaryFolder.Type = primaryFolder.Type;
                    secondaryFolder.Name = primaryFolder.Name;
                    secondaryFolder.Order = primaryFolder.Order;
                    secondaryFolder.ImageSizes = primaryFolder.ImageSizes;
                    secondaryFolder.Capacity = primaryFolder.Capacity;
                    secondaryFolder.ImageSizes = primaryFolder.ImageSizes;
                    secondaryFolder.IsSystem = primaryFolder.IsSystem;
                    secondaryFolder.PermissionList = ReplicatePermissions(primaryFolder.PermissionList, secondarySiteId);

                    if (folder == null)
                    {
                        if (siteGroup.Synchronize)
                        {
                            folderRepository.AddFolder(secondaryFolder);
                        }
                        log += Log(siteGroup, $"Folder Added: {secondaryFolder.Path}");
                    }
                    else
                    {
                        if (siteGroup.Synchronize)
                        {
                            folderRepository.UpdateFolder(secondaryFolder);
                        }
                        log += Log(siteGroup, $"Folder Updated: {secondaryFolder.Path}");
                        secondaryFolders.Remove(folder);
                    }
                }

                // folder settings
                log += ReplicateSettings(settingRepository, siteGroup, EntityNames.Folder, primaryFolder.FolderId, secondaryFolder.FolderId);

                // get files for folder
                var primaryFiles = fileRepository.GetFiles(primaryFolder.FolderId);
                var secondaryFiles = fileRepository.GetFiles(secondaryFolder.FolderId).ToList();

                foreach (var primaryFile in primaryFiles)
                {
                    var file = secondaryFiles.FirstOrDefault(item => item.Name == primaryFile.Name);

                    var secondaryFile = file;
                    if (secondaryFile == null)
                    {
                        secondaryFile = new Models.File();
                        secondaryFile.FolderId = secondaryFolder.FolderId;
                        secondaryFile.Name = primaryFile.Name;
                    }

                    if (file == null || primaryFile.ModifiedOn > siteGroup.SynchronizedOn)
                    {
                        // set all properties
                        secondaryFile.Extension = primaryFile.Extension;
                        secondaryFile.Size = primaryFile.Size;
                        secondaryFile.ImageHeight = primaryFile.ImageHeight;
                        secondaryFile.ImageWidth = primaryFile.ImageWidth;
                        secondaryFile.Description = primaryFile.Description;

                        if (file == null)
                        {
                            if (siteGroup.Synchronize)
                            {
                                fileRepository.AddFile(secondaryFile);
                                ReplicateFile(folderRepository, primaryFolder, primaryFile, secondaryFolder, secondaryFile);
                            }
                            log += Log(siteGroup, $"File Added: {CreateLink(siteGroup.AliasName + secondaryFolder.Path + secondaryFile.Name)}");
                        }
                        else
                        {
                            if (siteGroup.Synchronize)
                            {
                                fileRepository.UpdateFile(secondaryFile);
                                ReplicateFile(folderRepository, primaryFolder, primaryFile, secondaryFolder, secondaryFile);
                            }
                            log += Log(siteGroup, $"File Updated: {CreateLink(siteGroup.AliasName + secondaryFolder.Path + secondaryFile.Name)}");
                            secondaryFiles.Remove(file);
                        }
                    }
                }

                // remove files in the secondary site which do not exist in the primary site
                foreach (var secondaryFile in secondaryFiles.Where(item => !primaryFiles.Select(item => item.Name).Contains(item.Name)))
                {
                    if (siteGroup.Synchronize)
                    {
                        fileRepository.DeleteFile(secondaryFile.FileId);
                        var secondaryPath = Path.Combine(folderRepository.GetFolderPath(secondaryFolder), secondaryFile.Name);
                        System.IO.File.Delete(secondaryPath);
                    }
                    log += Log(siteGroup, $"File Deleted: {CreateLink(siteGroup.AliasName + secondaryFolder.Path + secondaryFile.Name)}");
                }
            }

            // remove folders in the secondary site which do not exist in the primary site
            foreach (var secondaryFolder in secondaryFolders.Where(item => !primaryFolders.Select(item => item.Path).Contains(item.Path)))
            {
                if (siteGroup.Synchronize)
                {
                    folderRepository.DeleteFolder(secondaryFolder.FolderId);
                }
                log += Log(siteGroup, $"Folder Deleted: {secondaryFolder.Path}");
            }

            return log;
        }

        private void ReplicateFile(IFolderRepository folderRepository, Folder primaryFolder, Models.File primaryFile, Folder secondaryFolder, Models.File secondaryFile)
        {
            var primaryPath = Path.Combine(folderRepository.GetFolderPath(primaryFolder), primaryFile.Name);
            var secondaryPath = Path.Combine(folderRepository.GetFolderPath(secondaryFolder), secondaryFile.Name);
            if (!Directory.Exists(Path.GetDirectoryName(secondaryPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(secondaryPath));
            }
            System.IO.File.Copy(primaryPath, secondaryPath, true);
        }

        private string ReplicatePages(IServiceProvider provider, ISettingRepository settingRepository, ITenantManager tenantManager, SiteGroup siteGroup, int primarySiteId, int secondarySiteId)
        {
            var pageRepository = provider.GetRequiredService<IPageRepository>();
            var pageModuleRepository = provider.GetRequiredService<IPageModuleRepository>();
            var moduleRepository = provider.GetRequiredService<IModuleRepository>();
            var log = "";

            List<PageModule> primaryPageModules = null;
            List<PageModule> secondaryPageModules = null;

            int tenantId = tenantManager.GetTenant().TenantId;

            // get pages (ignore personalized)
            var primaryPages = pageRepository.GetPages(primarySiteId).Where(item => item.UserId == null);
            var secondaryPages = pageRepository.GetPages(secondarySiteId).Where(item => item.UserId == null).ToList();

            // iterate through primary pages 
            foreach (var primaryPage in primaryPages)
            {
                var page = secondaryPages.FirstOrDefault(item => item.Path == primaryPage.Path);

                var secondaryPage = page;
                if (secondaryPage == null)
                {
                    secondaryPage = new Page();
                    secondaryPage.SiteId = secondarySiteId;
                }

                if (page == null || primaryPage.ModifiedOn > siteGroup.SynchronizedOn)
                {
                    // set all properties
                    secondaryPage.Path = primaryPage.Path;
                    secondaryPage.Name = primaryPage.Name;
                    secondaryPage.ParentId = null;
                    if (primaryPage.ParentId != null)
                    {
                        var parentPage = pageRepository.GetPage(primaryPages.First(item => item.PageId == primaryPage.ParentId).Path, secondarySiteId);
                        if (parentPage != null)
                        {
                            secondaryPage.ParentId = parentPage.PageId;
                        }
                    }
                    secondaryPage.Title = primaryPage.Title;
                    secondaryPage.Order = primaryPage.Order;
                    secondaryPage.Url = primaryPage.Url;
                    secondaryPage.ThemeType = primaryPage.ThemeType;
                    secondaryPage.DefaultContainerType = primaryPage.DefaultContainerType;
                    secondaryPage.HeadContent = primaryPage.HeadContent;
                    secondaryPage.BodyContent = primaryPage.BodyContent;
                    secondaryPage.Icon = primaryPage.Icon;
                    secondaryPage.IsNavigation = primaryPage.IsNavigation;
                    secondaryPage.IsClickable = primaryPage.IsClickable;
                    secondaryPage.UserId = null;
                    secondaryPage.IsPersonalizable = primaryPage.IsPersonalizable;
                    secondaryPage.EffectiveDate = primaryPage.EffectiveDate;
                    secondaryPage.ExpiryDate = primaryPage.ExpiryDate;
                    secondaryPage.CreatedBy = primaryPage.CreatedBy;
                    secondaryPage.CreatedOn = primaryPage.CreatedOn;
                    secondaryPage.ModifiedBy = primaryPage.ModifiedBy;
                    secondaryPage.ModifiedOn = primaryPage.ModifiedOn;
                    secondaryPage.DeletedBy = primaryPage.DeletedBy;
                    secondaryPage.DeletedOn = primaryPage.DeletedOn;
                    secondaryPage.IsDeleted = primaryPage.IsDeleted;
                    secondaryPage.PermissionList = ReplicatePermissions(primaryPage.PermissionList, secondarySiteId);

                    if (page == null)
                    {
                        if (siteGroup.Synchronize)
                        {
                            secondaryPage = pageRepository.AddPage(secondaryPage);
                        }
                        log += Log(siteGroup, $"Page Added: {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                    }
                    else
                    {
                        if (siteGroup.Synchronize)
                        {
                            secondaryPage = pageRepository.UpdatePage(secondaryPage);
                        }
                        log += Log(siteGroup, $"Page Updated: {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                        secondaryPages.Remove(page);
                    }
                }

                // page settings
                log += ReplicateSettings(settingRepository, siteGroup, EntityNames.Page, primaryPage.PageId, secondaryPage.PageId);

                // modules
                if (primaryPageModules == null)
                {
                    tenantManager.SetAlias(tenantId, primarySiteId); // required by ModuleDefinitionRepository.LoadModuleDefinitions()
                    primaryPageModules = pageModuleRepository.GetPageModules(primarySiteId).ToList();
                }
                if (secondaryPageModules == null)
                {
                    tenantManager.SetAlias(tenantId, secondarySiteId); // required by ModuleDefinitionRepository.LoadModuleDefinitions()
                    secondaryPageModules = pageModuleRepository.GetPageModules(secondarySiteId).ToList();
                }
                foreach (var primaryPageModule in primaryPageModules.Where(item => item.PageId == primaryPage.PageId))
                {
                    var pageModule = secondaryPageModules.FirstOrDefault(item => item.PageId == secondaryPage.PageId && item.Module.ModuleDefinitionName == primaryPageModule.Module.ModuleDefinitionName && item.Title.ToLower() == primaryPageModule.Title.ToLower());

                    var secondaryPageModule = pageModule;
                    if (secondaryPageModule == null)
                    {
                        secondaryPageModule = new PageModule();
                        secondaryPageModule.PageId = secondaryPage.PageId;
                        secondaryPageModule.Module = new Module();
                        secondaryPageModule.Module.SiteId = secondarySiteId;
                        secondaryPageModule.Module.ModuleDefinitionName = primaryPageModule.Module.ModuleDefinitionName;
                    }

                    if (pageModule == null || primaryPageModule.ModifiedOn > siteGroup.SynchronizedOn || primaryPageModule.Module.ModifiedOn > siteGroup.SynchronizedOn)
                    {
                        // set all properties
                        secondaryPageModule.Title = primaryPageModule.Title;
                        secondaryPageModule.Pane = primaryPageModule.Pane;
                        secondaryPageModule.Order = primaryPageModule.Order;
                        secondaryPageModule.ContainerType = primaryPageModule.ContainerType;
                        secondaryPageModule.Header = primaryPageModule.Header;
                        secondaryPageModule.Footer = primaryPageModule.Footer;
                        secondaryPageModule.IsDeleted = primaryPageModule.IsDeleted;
                        secondaryPageModule.Module.PermissionList = ReplicatePermissions(primaryPageModule.Module.PermissionList, secondarySiteId);
                        secondaryPageModule.Module.AllPages = false;
                        secondaryPageModule.Module.IsDeleted = false;

                        var updateContent = false;

                        if (pageModule == null)
                        {
                            // check if module exists
                            var module = secondaryPageModules.FirstOrDefault(item => item.Module.ModuleDefinitionName == primaryPageModule.Module.ModuleDefinitionName && item.Title.ToLower() == primaryPageModule.Title.ToLower())?.Module;
                            if (module == null)
                            {
                                // add new module
                                if (siteGroup.Synchronize)
                                {
                                    module = moduleRepository.AddModule(secondaryPageModule.Module);
                                    updateContent = true;
                                }
                                log += Log(siteGroup, $"Module Added: {module.Title} - {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                            }
                            if (module != null)
                            {
                                secondaryPageModule.ModuleId = module.ModuleId;
                                secondaryPageModule.Module = null; // remove tracking
                                if (siteGroup.Synchronize)
                                {
                                    secondaryPageModule = pageModuleRepository.AddPageModule(secondaryPageModule);
                                }
                                log += Log(siteGroup, $"Page Module Added: {module.Title} - {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                                secondaryPageModule.Module = module;
                            }
                        }
                        else
                        {
                            // update existing module
                            if (primaryPageModule.Module.ModifiedOn > siteGroup.SynchronizedOn)
                            {
                                if (siteGroup.Synchronize)
                                {
                                    moduleRepository.UpdateModule(secondaryPageModule.Module);
                                    updateContent = true;
                                }
                                log += Log(siteGroup, $"Module Updated: {secondaryPageModule.Title} - {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                            }
                            if (primaryPageModule.ModifiedOn > siteGroup.SynchronizedOn)
                            {
                                if (siteGroup.Synchronize)
                                {
                                    secondaryPageModule = pageModuleRepository.UpdatePageModule(secondaryPageModule);
                                }
                                log += Log(siteGroup, $"Page Module Updated: {secondaryPageModule.Title} - {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                                secondaryPageModules.Remove(pageModule);
                            }
                        }

                        // module content
                        if (updateContent && primaryPageModule.Module.ModuleDefinition.ServerManagerType != "")
                        {
                            Type moduleType = Type.GetType(primaryPageModule.Module.ModuleDefinition.ServerManagerType);
                            if (moduleType != null && moduleType.GetInterface(nameof(ISynchronizable)) != null)
                            {
                                try
                                {
                                    var moduleObject = ActivatorUtilities.CreateInstance(provider, moduleType);
                                    var primaryModuleContent = ((ISynchronizable)moduleObject).ExtractModule(primaryPageModule.Module);
                                    var secondaryModuleContent = ((ISynchronizable)moduleObject).ExtractModule(secondaryPageModule.Module);
                                    if (primaryModuleContent != secondaryModuleContent)
                                    {
                                        if (siteGroup.Synchronize)
                                        {
                                            ((ISynchronizable)moduleObject).LoadModule(secondaryPageModule.Module, primaryModuleContent, primaryPageModule.Module.ModuleDefinition.Version);
                                        }
                                        log += Log(siteGroup, $"Module Content Updated: {secondaryPageModule.Title} - {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
                                    }
                                }
                                catch
                                {
                                    // error exporting/importing
                                }
                            }
                        }
                    }

                    // module settings
                    log += ReplicateSettings(settingRepository, siteGroup, EntityNames.Module, primaryPageModule.ModuleId, secondaryPageModule.ModuleId);
                }
            }

            // remove modules in the secondary site which do not exist in the primary site
            foreach (var secondaryPageModule in secondaryPageModules)
            {
                var primaryPageId = -1;
                var secondaryPage = secondaryPages.FirstOrDefault(item => item.PageId == secondaryPageModule.PageId);
                if (secondaryPage != null)
                {
                    var primaryPage = primaryPages.FirstOrDefault(item => item.Path == secondaryPage.Path);
                    if (primaryPage != null)
                    {
                        primaryPageId = primaryPage.PageId;
                    }
                }
                if (!primaryPageModules.Any(item => item.PageId == primaryPageId && item.Module.ModuleDefinitionName == secondaryPageModule.Module.ModuleDefinitionName && item.Title.ToLower() == secondaryPageModule.Title.ToLower()))
                {
                    if (siteGroup.Synchronize)
                    {
                        pageModuleRepository.DeletePageModule(secondaryPageModule.PageModuleId);
                    }
                    log += Log(siteGroup, $"Page Module Deleted: {secondaryPageModule.Title} - {CreateLink(siteGroup.AliasName + secondaryPageModule.Page.Path)}");
                }
            }

            // remove pages in the secondary site which do not exist in the primary site
            foreach (var secondaryPage in secondaryPages.Where(item => !primaryPages.Select(item => item.Path).Contains(item.Path)))
            {
                if (siteGroup.Synchronize)
                {
                    pageRepository.DeletePage(secondaryPage.PageId);
                }
                log += Log(siteGroup, $"Page Deleted: {CreateLink(siteGroup.AliasName + secondaryPage.Path)}");
            }

            if (siteGroup.SynchronizedOn == DateTime.MinValue || !string.IsNullOrEmpty(log))
            {
                // clear cache for secondary site if any content was replicated
                var syncManager = provider.GetRequiredService<ISyncManager>();
                var alias = new Alias { TenantId = tenantManager.GetTenant().TenantId, SiteId = secondarySiteId };
                syncManager.AddSyncEvent(alias, EntityNames.Site, secondarySiteId, SyncEventActions.Refresh);
            }

            return log;
        }

        private List<Permission> ReplicatePermissions(List<Permission> permissionList, int siteId)
        {
            return permissionList.Select(item => new Permission
            {
                SiteId = siteId,
                PermissionName = item.PermissionName,
                RoleName = item.RoleName,
                UserId = item.UserId,
                IsAuthorized = item.IsAuthorized,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,
                ModifiedBy = item.ModifiedBy,
                ModifiedOn = item.ModifiedOn
            }).ToList();
        }

        private string ReplicateSettings(ISettingRepository settingRepository, SiteGroup siteGroup, string entityName, int primaryEntityId, int secondaryEntityId)
        {
            var log = "";
            var updated = false;

            var secondarySettings = settingRepository.GetSettings(entityName, secondaryEntityId).ToList();
            foreach (var primarySetting in settingRepository.GetSettings(entityName, primaryEntityId))
            {
                var secondarySetting = secondarySettings.FirstOrDefault(item => item.SettingName == primarySetting.SettingName);
                if (secondarySetting == null)
                {
                    secondarySetting = new Setting();
                    secondarySetting.EntityName = primarySetting.EntityName;
                    secondarySetting.EntityId = secondaryEntityId;
                    secondarySetting.SettingName = primarySetting.SettingName;
                    secondarySetting.SettingValue = primarySetting.SettingValue;
                    secondarySetting.IsPrivate = primarySetting.IsPrivate;
                    if (siteGroup.Synchronize && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
                    {
                        settingRepository.AddSetting(secondarySetting);
                        updated = true;
                    }
                }
                else
                {
                    if (secondarySetting.SettingValue != primarySetting.SettingValue || secondarySetting.IsPrivate != primarySetting.IsPrivate)
                    {
                        secondarySetting.SettingValue = primarySetting.SettingValue;
                        secondarySetting.IsPrivate = primarySetting.IsPrivate;
                        if (siteGroup.Synchronize && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
                        {
                            settingRepository.UpdateSetting(secondarySetting);
                            updated = true;
                        }
                    }
                    secondarySettings.Remove(secondarySetting);
                }
            }

            // any remaining secondary settings need to be deleted
            foreach (var secondarySetting in secondarySettings)
            {
                if (siteGroup.Synchronize && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
                {
                    settingRepository.DeleteSetting(secondarySetting.EntityName, secondarySetting.SettingId);
                    updated = true;
                }
            }

            if (updated)
            {
                log += Log(siteGroup, $"{entityName} Settings Updated");
            }

            return log;
        }

        private void SendNotifications(IServiceProvider provider, int siteId, string siteName, string log)
        {
            var userRoleRepository = provider.GetRequiredService<IUserRoleRepository>();
            var notificationRepository = provider.GetRequiredService<INotificationRepository>();

            foreach (var userRole in userRoleRepository.GetUserRoles(RoleNames.Admin, siteId))
            {
                var notification = new Notification(siteId, userRole.User, $"{siteName} Change Log", log);
                notificationRepository.AddNotification(notification);
            }
        }

        private string Log(SiteGroup siteGroup, string content)
        {
            // not necessary to log initial replication
            if (siteGroup.SynchronizedOn != DateTime.MinValue)
            {
                return content + "<br />";
            }
            else
            {
                return "";
            }
        }

        private string CreateLink(string url)
        {
            return "<a href=\"" + url + "\" target=\"_new\">" + url + "</a>";
        }
    }
}
