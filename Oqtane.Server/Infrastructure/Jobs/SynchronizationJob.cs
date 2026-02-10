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

            var siteGroupRepository = provider.GetRequiredService<ISiteGroupRepository>();
            var siteGroupMemberRepository = provider.GetRequiredService<ISiteGroupMemberRepository>();
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var aliasRepository = provider.GetRequiredService<IAliasRepository>();
            var tenantManager = provider.GetRequiredService<ITenantManager>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();

            List<SiteGroupMember> siteGroupMembers = null;
            List<Site> sites = null;
            List<Alias> aliases = null;

            // get site groups
            var siteGroups = siteGroupRepository.GetSiteGroups();

            // iterate through site groups which need to be synchronized
            foreach (var siteGroup in siteGroups.Where(item => item.Synchronize && (item.Type == SiteGroupTypes.Synchronization || item.Type == SiteGroupTypes.Comparison)))
            {
                // get data
                if (siteGroupMembers == null)
                {
                    siteGroupMembers = siteGroupMemberRepository.GetSiteGroupMembers().ToList();
                    sites = siteRepository.GetSites().ToList();
                    aliases = aliasRepository.GetAliases().ToList();
                }

                var aliasName = "https://" + aliases.First(item => item.TenantId == tenantManager.GetTenant().TenantId && item.SiteId == siteGroup.PrimarySiteId && item.IsDefault).Name;
                log += (siteGroup.Type == SiteGroupTypes.Synchronization) ? "Synchronizing " : "Comparing ";
                log += $"Primary Site: {sites.First(item => item.SiteId == siteGroup.PrimarySiteId).Name} - {CreateLink(aliasName)}<br />";

                // get primary site
                var primarySite = sites.FirstOrDefault(item => item.SiteId == siteGroup.PrimarySiteId);
                if (primarySite != null)
                {
                    // update flag to prevent job from processing group again
                    siteGroup.Synchronize = false;
                    siteGroupRepository.UpdateSiteGroup(siteGroup);

                    // iterate through sites in site group
                    foreach (var siteGroupMember in siteGroupMembers.Where(item => item.SiteGroupId == siteGroup.SiteGroupId && item.SiteId != siteGroup.PrimarySiteId))
                    {
                        // get secondary site
                        var secondarySite = sites.FirstOrDefault(item => item.SiteId == siteGroupMember.SiteId);
                        if (secondarySite != null)
                        {
                            // get default alias for site
                            if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                            {
                                siteGroupMember.AliasName = "https://" + aliases.First(item => item.TenantId == tenantManager.GetTenant().TenantId && item.SiteId == siteGroupMember.SiteId && item.IsDefault).Name;
                            }
                            else
                            {
                                siteGroupMember.AliasName = aliasName;
                            }

                            // initialize SynchronizedOn
                            if (siteGroupMember.SynchronizedOn == null)
                            {
                                siteGroupMember.SynchronizedOn = DateTime.MinValue;
                            }

                            // synchronize site
                            var siteLog = SynchronizeSite(provider, tenantManager, settingRepository, siteGroupMember, primarySite, secondarySite);

                            // set synchronized on date/time
                            siteGroupMember.SynchronizedOn = DateTime.UtcNow;
                            siteGroupMemberRepository.UpdateSiteGroupMember(siteGroupMember);

                            log += $"With Secondary Site: {secondarySite.Name} - {CreateLink(siteGroupMember.AliasName)}<br />" + siteLog;
                        }
                        else
                        {
                            log += $"Site Group {siteGroup.Name} Has A SiteId {siteGroupMember.SiteId} Which Does Not Exist<br />";
                        }
                    }
                }
                else
                {
                    log += $"Site Group {siteGroup.Name} Has A PrimarySiteId {siteGroup.PrimarySiteId} Which Does Not Exist<br />";
                }
            }

            if (string.IsNullOrEmpty(log))
            {
                log = "No Site Groups Require Synchronization<br />";
            }

            return log;
        }

        private string SynchronizeSite(IServiceProvider provider, ITenantManager tenantManager, ISettingRepository settingRepository, SiteGroupMember siteGroupMember, Site primarySite, Site secondarySite)
        {
            var log = "";

            // synchronize roles/users
            log += SynchronizeRoles(provider, settingRepository, siteGroupMember, primarySite.SiteId, secondarySite.SiteId);

            // synchronize folders/files
            log += SynchronizeFolders(provider, settingRepository, siteGroupMember, primarySite.SiteId, secondarySite.SiteId);

            // synchronize pages/modules
            log += SynchronizePages(provider, settingRepository, tenantManager, siteGroupMember, primarySite.SiteId, secondarySite.SiteId);

            // synchronize site
            if (primarySite.ModifiedOn > siteGroupMember.SynchronizedOn)
            {
                secondarySite.TimeZoneId = primarySite.TimeZoneId;
                secondarySite.CultureCode = primarySite.CultureCode;
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

                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                {
                    var siteRepository = provider.GetRequiredService<ISiteRepository>();
                    siteRepository.UpdateSite(secondarySite);
                }
                log += Log(siteGroupMember, $"Site Updated: {secondarySite.Name} - {CreateLink(siteGroupMember.AliasName)}");
            }

            // site settings
            log += SynchronizeSettings(settingRepository, siteGroupMember, EntityNames.Site, primarySite.SiteId, secondarySite.SiteId);

            if (siteGroupMember.SynchronizedOn == DateTime.MinValue || !string.IsNullOrEmpty(log))
            {
                // clear cache for secondary site if any content was Synchronized
                var syncManager = provider.GetRequiredService<ISyncManager>();
                var alias = new Alias { TenantId = tenantManager.GetTenant().TenantId, SiteId = secondarySite.SiteId }; 
                syncManager.AddSyncEvent(alias, EntityNames.Site, secondarySite.SiteId, SyncEventActions.Refresh);
            }

            if (!string.IsNullOrEmpty(log) && siteGroupMember.SiteGroup.Type == SiteGroupTypes.Comparison)
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

        private string SynchronizeRoles(IServiceProvider provider, ISettingRepository settingRepository, SiteGroupMember siteGroupMember, int primarySiteId, int secondarySiteId)
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

                if (role == null || primaryRole.ModifiedOn > siteGroupMember.SynchronizedOn)
                {
                    // set all properties
                    secondaryRole.Name = primaryRole.Name;
                    secondaryRole.Description = primaryRole.Description;
                    secondaryRole.IsAutoAssigned = primaryRole.IsAutoAssigned;
                    secondaryRole.IsSystem = primaryRole.IsSystem;

                    if (role == null)
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            roleRepository.AddRole(secondaryRole);
                        }
                        log += Log(siteGroupMember, $"Role Added: {secondaryRole.Name}");
                    }
                    else
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            roleRepository.UpdateRole(secondaryRole);
                        }
                        log += Log(siteGroupMember, $"Role Updated: {secondaryRole.Name}");
                    }
                }

                if (role != null)
                {
                    secondaryRoles.Remove(role);
                }
            }

            // remove roles in the secondary site which do not exist in the primary site
            foreach (var secondaryRole in secondaryRoles)
            {
                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                {
                    roleRepository.DeleteRole(secondaryRole.RoleId);
                }
                log += Log(siteGroupMember, $"Role Deleted: {secondaryRole.Name}");
            }

            // settings
            log += SynchronizeSettings(settingRepository, siteGroupMember, EntityNames.Role, primarySiteId, secondarySiteId);

            return log;
        }

        private string SynchronizeFolders(IServiceProvider provider, ISettingRepository settingRepository, SiteGroupMember siteGroupMember, int primarySiteId, int secondarySiteId)
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

                if (folder == null || primaryFolder.ModifiedOn > siteGroupMember.SynchronizedOn)
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
                    secondaryFolder.PermissionList = SynchronizePermissions(primaryFolder.PermissionList, secondarySiteId);

                    if (folder == null)
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            folderRepository.AddFolder(secondaryFolder);
                        }
                        log += Log(siteGroupMember, $"Folder Added: {secondaryFolder.Path}");
                    }
                    else
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            folderRepository.UpdateFolder(secondaryFolder);
                        }
                        log += Log(siteGroupMember, $"Folder Updated: {secondaryFolder.Path}");
                    }
                }

                if (folder != null)
                {
                    secondaryFolders.Remove(folder);
                }

                // folder settings
                log += SynchronizeSettings(settingRepository, siteGroupMember, EntityNames.Folder, primaryFolder.FolderId, secondaryFolder.FolderId);

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

                    if (file == null || primaryFile.ModifiedOn > siteGroupMember.SynchronizedOn)
                    {
                        // set all properties
                        secondaryFile.Extension = primaryFile.Extension;
                        secondaryFile.Size = primaryFile.Size;
                        secondaryFile.ImageHeight = primaryFile.ImageHeight;
                        secondaryFile.ImageWidth = primaryFile.ImageWidth;
                        secondaryFile.Description = primaryFile.Description;

                        if (file == null)
                        {
                            if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                            {
                                fileRepository.AddFile(secondaryFile);
                                SynchronizeFile(folderRepository, primaryFolder, primaryFile, secondaryFolder, secondaryFile);
                            }
                            log += Log(siteGroupMember, $"File Added: {CreateLink(siteGroupMember.AliasName + "/" + secondaryFolder.Path + secondaryFile.Name)}");
                        }
                        else
                        {
                            if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                            {
                                fileRepository.UpdateFile(secondaryFile);
                                SynchronizeFile(folderRepository, primaryFolder, primaryFile, secondaryFolder, secondaryFile);
                            }
                            log += Log(siteGroupMember, $"File Updated: {CreateLink(siteGroupMember.AliasName + "/" + secondaryFolder.Path + secondaryFile.Name)}");
                        }
                    }

                    if (file != null)
                    {
                        secondaryFiles.Remove(file);
                    }
                }

                // remove files in the secondary site which do not exist in the primary site
                foreach (var secondaryFile in secondaryFiles)
                {
                    if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                    {
                        fileRepository.DeleteFile(secondaryFile.FileId);
                        var secondaryPath = Path.Combine(folderRepository.GetFolderPath(secondaryFolder), secondaryFile.Name);
                        System.IO.File.Delete(secondaryPath);
                    }
                    log += Log(siteGroupMember, $"File Deleted: {CreateLink(siteGroupMember.AliasName + "/" + secondaryFolder.Path + secondaryFile.Name)}");
                }
            }

            // remove folders in the secondary site which do not exist in the primary site
            foreach (var secondaryFolder in secondaryFolders)
            {
                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                {
                    folderRepository.DeleteFolder(secondaryFolder.FolderId);
                }
                log += Log(siteGroupMember, $"Folder Deleted: {secondaryFolder.Path}");
            }

            return log;
        }

        private void SynchronizeFile(IFolderRepository folderRepository, Folder primaryFolder, Models.File primaryFile, Folder secondaryFolder, Models.File secondaryFile)
        {
            var primaryPath = Path.Combine(folderRepository.GetFolderPath(primaryFolder), primaryFile.Name);
            if (System.IO.File.Exists(primaryPath))
            {
                var secondaryPath = Path.Combine(folderRepository.GetFolderPath(secondaryFolder), secondaryFile.Name);
                if (!Directory.Exists(Path.GetDirectoryName(secondaryPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(secondaryPath));
                }
                System.IO.File.Copy(primaryPath, secondaryPath, true);
            }
        }

        private string SynchronizePages(IServiceProvider provider, ISettingRepository settingRepository, ITenantManager tenantManager, SiteGroupMember siteGroupMember, int primarySiteId, int secondarySiteId)
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

                if (page == null || primaryPage.ModifiedOn > siteGroupMember.SynchronizedOn)
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
                    secondaryPage.PermissionList = SynchronizePermissions(primaryPage.PermissionList, secondarySiteId);

                    if (page == null)
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            secondaryPage = pageRepository.AddPage(secondaryPage);
                        }
                        log += Log(siteGroupMember, $"Page Added: {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                    }
                    else
                    {
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                        {
                            secondaryPage = pageRepository.UpdatePage(secondaryPage);
                        }
                        log += Log(siteGroupMember, $"Page Updated: {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                    }
                }

                if (page != null)
                {
                    secondaryPages.Remove(page);
                }

                // page settings
                log += SynchronizeSettings(settingRepository, siteGroupMember, EntityNames.Page, primaryPage.PageId, secondaryPage.PageId);

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

                    if (pageModule == null || primaryPageModule.ModifiedOn > siteGroupMember.SynchronizedOn || primaryPageModule.Module.ModifiedOn > siteGroupMember.SynchronizedOn)
                    {
                        // set all properties
                        secondaryPageModule.Title = primaryPageModule.Title;
                        secondaryPageModule.Pane = primaryPageModule.Pane;
                        secondaryPageModule.Order = primaryPageModule.Order;
                        secondaryPageModule.ContainerType = primaryPageModule.ContainerType;
                        secondaryPageModule.Header = primaryPageModule.Header;
                        secondaryPageModule.Footer = primaryPageModule.Footer;
                        secondaryPageModule.IsDeleted = primaryPageModule.IsDeleted;
                        secondaryPageModule.Module.PermissionList = SynchronizePermissions(primaryPageModule.Module.PermissionList, secondarySiteId);
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
                                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                                {
                                    module = moduleRepository.AddModule(secondaryPageModule.Module);
                                    updateContent = true;
                                }
                                log += Log(siteGroupMember, $"Module Added: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                            }
                            if (module != null)
                            {
                                secondaryPageModule.ModuleId = module.ModuleId;
                                secondaryPageModule.Module = null; // remove tracking
                                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                                {
                                    secondaryPageModule = pageModuleRepository.AddPageModule(secondaryPageModule);
                                }
                                log += Log(siteGroupMember, $"Module Instance Added: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                                secondaryPageModule.Module = module;
                            }
                        }
                        else
                        {
                            // update existing module
                            if (primaryPageModule.Module.ModifiedOn > siteGroupMember.SynchronizedOn)
                            {
                                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                                {
                                    moduleRepository.UpdateModule(secondaryPageModule.Module);
                                }
                                updateContent = true;
                                log += Log(siteGroupMember, $"Module Updated: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                            }
                            if (primaryPageModule.ModifiedOn > siteGroupMember.SynchronizedOn)
                            {
                                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                                {
                                    secondaryPageModule = pageModuleRepository.UpdatePageModule(secondaryPageModule);
                                }
                                log += Log(siteGroupMember, $"Module Instance Updated: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
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
                                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                                        {
                                            ((ISynchronizable)moduleObject).LoadModule(secondaryPageModule.Module, primaryModuleContent, primaryPageModule.Module.ModuleDefinition.Version);
                                        }
                                        log += Log(siteGroupMember, $"Module Content Updated: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
                                    }
                                }
                                catch
                                {
                                    // error exporting/importing
                                }
                            }
                        }
                    }

                    if (pageModule != null)
                    {
                        secondaryPageModules.Remove(pageModule);
                    }

                    // module settings
                    log += SynchronizeSettings(settingRepository, siteGroupMember, EntityNames.Module, primaryPageModule.ModuleId, secondaryPageModule.ModuleId);
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
                    if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                    {
                        pageModuleRepository.DeletePageModule(secondaryPageModule.PageModuleId);
                    }
                    log += Log(siteGroupMember, $"Module Instance Deleted: {secondaryPageModule.Title} - {CreateLink(siteGroupMember.AliasName + "/" + secondaryPageModule.Page.Path)}");
                }
            }

            // remove pages in the secondary site which do not exist in the primary site
            foreach (var secondaryPage in secondaryPages)
            {
                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization)
                {
                    pageRepository.DeletePage(secondaryPage.PageId);
                }
                log += Log(siteGroupMember, $"Page Deleted: {CreateLink(siteGroupMember.AliasName + "/" + secondaryPage.Path)}");
            }

            if (siteGroupMember.SynchronizedOn == DateTime.MinValue || !string.IsNullOrEmpty(log))
            {
                // clear cache for secondary site if any content was Synchronized
                var syncManager = provider.GetRequiredService<ISyncManager>();
                var alias = new Alias { TenantId = tenantManager.GetTenant().TenantId, SiteId = secondarySiteId };
                syncManager.AddSyncEvent(alias, EntityNames.Site, secondarySiteId, SyncEventActions.Refresh);
            }

            return log;
        }

        private List<Permission> SynchronizePermissions(List<Permission> permissionList, int siteId)
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

        private string SynchronizeSettings(ISettingRepository settingRepository, SiteGroupMember siteGroupMember, string entityName, int primaryEntityId, int secondaryEntityId)
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
                    if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
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
                        if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
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
                if (siteGroupMember.SiteGroup.Type == SiteGroupTypes.Synchronization && !excludedSettings.Any(item => item.EntityName == secondarySetting.EntityName && item.SettingName == secondarySetting.SettingName))
                {
                    settingRepository.DeleteSetting(secondarySetting.EntityName, secondarySetting.SettingId);
                    updated = true;
                }
            }

            if (updated)
            {
                log += Log(siteGroupMember, $"{entityName} Settings Updated");
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

        private string Log(SiteGroupMember siteGroupMember, string content)
        {
            // not necessary to log initial synchronization
            if (siteGroupMember.SynchronizedOn != DateTime.MinValue)
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
