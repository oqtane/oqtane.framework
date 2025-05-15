using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System.Net;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class FolderController : Controller
    {
        private readonly IFolderRepository _folders;
        private readonly IUserPermissions _userPermissions;
        private readonly IFileRepository _files;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public FolderController(IFolderRepository folders, IUserPermissions userPermissions, IFileRepository files, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _folders = folders;
            _userPermissions = userPermissions;
            _files = files;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Folder> Get(string siteid)
        {
            List<Folder> folders = new List<Folder>();
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                var hierarchy = GetFoldersHierarchy(_folders.GetFolders(SiteId).ToList());
                foreach (Folder folder in hierarchy)
                {
                    // note that Browse permission is used for this method
                    if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.PermissionList))
                    {
                        folders.Add(folder);
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
            Folder folder = _folders.GetFolder(siteId, folderPath);
            if (folder == null && User.IsInRole(RoleNames.Host) && path.StartsWith("Users/"))
            {
                // create the user folder on this site for the host user
                var userId = int.Parse(path.ReplaceMultiple(new string[] { "Users", "/" }, ""));
                folder = _folders.GetFolder(siteId, "Users/");
                if (folder != null)
                {
                    folder = _folders.AddFolder(new Folder
                    {
                        SiteId = folder.SiteId,
                        ParentId = folder.FolderId,
                        Name = "My Folder",
                        Type = FolderTypes.Private,
                        Path = path,
                        Order = 1,
                        ImageSizes = "",
                        Capacity = Constants.UserFolderCapacity,
                        IsSystem = true,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.Browse, userId, true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, userId, true)
                        }
                    });
                }
            }
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
        public Folder Post([FromBody] Folder folder)
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
                        folder = _folders.AddFolder(folder);
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
        public Folder Put(int id, [FromBody] Folder folder)
        {
            if (ModelState.IsValid && folder.SiteId == _alias.SiteId && folder.FolderId == id && _folders.GetFolder(folder.FolderId, false) != null && _userPermissions.IsAuthorized(User, folder.SiteId, EntityNames.Folder, folder.FolderId, PermissionNames.Edit))
            {
                if (folder.IsPathValid())
                {
                    if (folder.ParentId != null)
                    {
                        Folder parent = _folders.GetFolder(folder.ParentId.Value);
                        folder.Path = Utilities.UrlCombine(parent.Path, folder.Name);
                    }
                    if (!folder.Path.EndsWith("/"))
                    {
                        folder.Path = folder.Path + "/";
                    }

                    Folder _folder = _folders.GetFolder(id, false);
                    if (_folder.Path != folder.Path && Directory.Exists(_folders.GetFolderPath(_folder)))
                    {
                        Directory.Move(_folders.GetFolderPath(_folder), _folders.GetFolderPath(folder));
                    }

                    folder = _folders.UpdateFolder(folder);
                    _syncManager.AddSyncEvent(_alias, EntityNames.Folder, folder.FolderId, SyncEventActions.Update);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Updated {Folder}", folder);
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

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            var folder = _folders.GetFolder(id, false);
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, folder.SiteId, EntityNames.Folder, id, PermissionNames.Edit))
            {
                var folderPath = _folders.GetFolderPath(folder);
                if (Directory.Exists(folderPath))
                {
                    // remove all files from disk (including thumbnails, etc...)
                    foreach (var filePath in Directory.GetFiles(folderPath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    Directory.Delete(folderPath);
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

        private static List<Folder> GetFoldersHierarchy(List<Folder> folders)
        {
            List<Folder> hierarchy = new List<Folder>();
            Action<List<Folder>, Folder> getPath = null;
            getPath = (folderList, folder) =>
            {
                IEnumerable<Folder> children;
                int level;
                if (folder == null)
                {
                    level = -1;
                    children = folders.Where(item => item.ParentId == null);
                }
                else
                {
                    level = folder.Level;
                    children = folders.Where(item => item.ParentId == folder.FolderId);
                }

                foreach (Folder child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = folders.Any(item => item.ParentId == child.FolderId);
                    hierarchy.Add(child);
                    getPath(folderList, child);
                }
            };
            folders = folders.OrderBy(item => item.Name).ToList();
            getPath(folders, null);

            // add any non-hierarchical items to the end of the list
            foreach (Folder folder in folders)
            {
                if (hierarchy.Find(item => item.FolderId == folder.FolderId) == null)
                {
                    hierarchy.Add(folder);
                }
            }

            return hierarchy;
        }
    }
}
