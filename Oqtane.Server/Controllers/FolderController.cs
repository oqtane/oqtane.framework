using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Managers;
using Oqtane.Models;
using Oqtane.Providers;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class FolderController : Controller
    {
        private readonly IFolderProviderFactory _folderProviderFactory;
        private readonly IFolderRepository _folders;
        private readonly IFolderConfigRepository _folderConfigs;
        private readonly IUserPermissions _userPermissions;
        private readonly IFileRepository _files;
        private readonly IFolderManager _folderManager;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public FolderController(
            IFolderProviderFactory folderProviderFactory,
            IFolderRepository folders,
            IFolderConfigRepository folderConfigs,
            IUserPermissions userPermissions,
            IFileRepository files,
            IFolderManager folderManager,
            ISyncManager syncManager,
            ILogManager logger,
            ITenantManager tenantManager,
            IPermissionRepository permissionRepository)
        {
            _folderProviderFactory = folderProviderFactory;
            _folders = folders;
            _folderConfigs = folderConfigs;
            _userPermissions = userPermissions;
            _files = files;
            _folderManager = folderManager;
            _permissionRepository = permissionRepository;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&includeuserfolder=y
        [HttpGet]
        public IEnumerable<Folder> Get(string siteid, string includeuserfolder)
        {
            bool includeUserFolder = true;
            if (!string.IsNullOrEmpty(includeuserfolder) && bool.TryParse(includeuserfolder, out bool result))
            {
                includeUserFolder = result;
            }

            List<Folder> folders = new List<Folder>();
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                var hierarchy = _folders.GetFolders(SiteId, _userPermissions.GetUser(User).UserId).ToList();
                foreach (Folder folder in hierarchy)
                {
                    // note that Browse permission is used for this method
                    if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.PermissionList))
                    {
                        if (includeUserFolder || !folder.Path.StartsWith(Constants.UserFolderPath))
                        {
                            folders.Add(folder);
                        }
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folders = null;
            }
            return folders;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Folder Get(int id)
        {
            Folder folder = _folders.GetFolder(id);
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, folder.PermissionList))
            {
                return folder;
            }
            else
            {
                if (folder != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Get Attempt {FolderId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // GET api/<controller>/path/x/?path=y
        [HttpGet("path/{siteId}")]
        public Folder GetByPath(int siteId, string path)
        {
            var folderPath = WebUtility.UrlDecode(path).Replace("\\", "/"); // handle legacy path format
            folderPath = (folderPath == "/") ? "" : folderPath;
            if (!folderPath.EndsWith("/") && folderPath != "")
            {
                folderPath += "/";
            }
            Folder folder = _folders.GetFolder(siteId, folderPath, _userPermissions.GetUser(User).UserId);
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, folder.PermissionList))
            {
                return folder;
            }
            else
            {
                if (folder != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Get Attempt {Path} For Site {SiteId}", path, siteId);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task<Folder> Post([FromBody] Folder folder)
        {
            if (ModelState.IsValid && folder.SiteId == _alias.SiteId)
            {
                List<Permission> permissions;
                if (folder.ParentId != null)
                {
                    permissions = _folders.GetFolder(folder.ParentId.Value).PermissionList;
                }
                else
                {
                    permissions = new List<Permission> {
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true),
                    };
                }
                if (_userPermissions.IsAuthorized(User, PermissionNames.Edit, permissions))
                {
                    if (folder.IsPathValid())
                    {
                        if (string.IsNullOrEmpty(folder.Path) && folder.ParentId != null)
                        {
                            Folder parent = _folders.GetFolder(folder.ParentId.Value);
                            folder.Path = Utilities.UrlCombine(parent.Path, folder.Name);
                        }
                        if (!folder.Path.EndsWith("/"))
                        {
                            folder.Path = folder.Path + "/";
                        }
                        if(folder.FolderConfigId <= 0)
                        {
                            folder.FolderConfigId = _folderProviderFactory.GetDefaultConfigId(folder.SiteId);
                        }

                        folder = _folders.AddFolder(folder);
                        //create the folder in the provider
                        var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                        await folderProvider.CreateFolderAsync(folder);
                        _syncManager.AddSyncEvent(_alias, EntityNames.Folder, folder.FolderId, SyncEventActions.Create);
                        _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Added {Folder}", folder);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Name Not Valid {Folder}", folder);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        folder = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Post Attempt {Folder}", folder);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    folder = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Post Attempt {Folder}", folder);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folder = null;
            }
            return folder;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task<Folder> PutAsync(int id, [FromBody] Folder folder)
        {
            if (ModelState.IsValid && folder.SiteId == _alias.SiteId && folder.FolderId == id && _folders.GetFolder(folder.FolderId, false) != null && _userPermissions.IsAuthorized(User, folder.SiteId, EntityNames.Folder, folder.FolderId, PermissionNames.Edit))
            {
                if (folder.IsPathValid())
                {
                    if (folder.ParentId != null)
                    {
                        Folder parent = _folders.GetFolder(folder.ParentId.Value);

                        if(parent.FolderConfigId != _folderProviderFactory.GetDefaultConfigId(folder.SiteId) && folder.FolderConfigId != parent.FolderConfigId)
                        {
                            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Config Not Valid {Folder}", folder);
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return null;
                        }

                        folder.Path = Utilities.UrlCombine(parent.Path, folder.Name);
                        if (!folder.Path.EndsWith("/"))
                        {
                            folder.Path = folder.Path + "/";
                        }
                    }

                    var _folder = _folders.GetFolder(id, false);
                    folder = _folders.UpdateFolder(folder);

                    if (folder.Path != _folder.Path) // need to update all child folder's path
                    {
                        UpdateChildFoldersPath(folder);
                    }

                    var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                    if (_folder.MappedPath != folder.MappedPath && _folder.FolderConfigId == folder.FolderConfigId && await folderProvider.FolderExistsAsync(_folder))
                    {
                        await folderProvider.MoveFolderAsync(_folder, folder.MappedPath);
                    }

                    _syncManager.AddSyncEvent(_alias, EntityNames.Folder, folder.FolderId, SyncEventActions.Update);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Updated {Folder}", folder);

                    if (folder.UpdateSubfolderPermissions)
                    {
                        UpdateSubfoldersRecursively(folder.SiteId, folder.FolderId);
                        _logger.Log(LogLevel.Information, this, LogFunction.Update, "Subfolder Permissions Updated {Folder}", folder);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Name Not Valid {Folder}", folder);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    folder = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Put Attempt  {Folder}", folder);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                folder = null;
            }

            return folder;
        }

        private void UpdateSubfoldersRecursively(int siteId, int parentId)
        {
            var permissions = _permissionRepository.GetPermissions(siteId, EntityNames.Folder, parentId).ToList();

            foreach (var subfolder in _folders.GetFolders(siteId).Where(item => item.ParentId == parentId).ToList())
            {
                // remove existing permissions
                _permissionRepository.DeletePermissions(siteId, EntityNames.Folder, subfolder.FolderId);

                // add parent permissions
                foreach (Permission permission in permissions)
                {
                    _permissionRepository.AddPermission(new Permission
                    {
                        SiteId = siteId,
                        EntityName = EntityNames.Folder,
                        EntityId = subfolder.FolderId,
                        PermissionName = permission.PermissionName,
                        RoleId = permission.RoleId,
                        UserId = permission.UserId,
                        IsAuthorized = permission.IsAuthorized
                    });
                }

                _syncManager.AddSyncEvent(_alias, EntityNames.Folder, subfolder.FolderId, SyncEventActions.Update);

                if (_folders.GetFolders(siteId).Where(item => item.ParentId == subfolder.FolderId).Any())
                {
                    UpdateSubfoldersRecursively(siteId, subfolder.FolderId);
                }
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task Delete(int id)
        {
            var folder = _folders.GetFolder(id, false);
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, folder.SiteId, EntityNames.Folder, id, PermissionNames.Edit))
            {
                var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                if (folderProvider != null)
                {
                    await folderProvider.DeleteFolderAsync(folder);
                }

                // remove files from database
                foreach (var file in _files.GetFiles(id))
                {
                    _files.DeleteFile(file.FileId);
                }

                _folders.DeleteFolder(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.Folder, folder.FolderId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Folder Deleted {FolderId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Delete Attempt {FolderId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [HttpPost("sync/{id}/{recursive}/{includeFiles}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task SyncFolder(int id, bool recursive, bool includeFiles)
        {
            var folder = _folders.GetFolder(id, false);
            if (folder != null && folder.SiteId == _alias.SiteId)
            {
                await _folderManager.SyncFolderAsync(folder, recursive, includeFiles);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Folder Sync Attempt {Folder}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private void UpdateChildFoldersPath(Folder folder)
        {
            var childFolders = _folders.GetFolders(folder.SiteId).Where(i => i.ParentId == folder.FolderId);
            foreach(var childFolder in childFolders)
            {
                childFolder.Path = Utilities.UrlCombine(folder.Path, childFolder.Name);
                if (!childFolder.Path.EndsWith("/"))
                {
                    childFolder.Path = childFolder.Path + "/";
                }
                _folders.UpdateFolder(childFolder);

                UpdateChildFoldersPath(childFolder);
            }
        }
    }
}
