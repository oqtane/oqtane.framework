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
using Microsoft.AspNetCore.Hosting;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class FolderController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFolderRepository _folders;
        private readonly IUserPermissions _userPermissions;
        private readonly ITenantResolver _tenants;
        private readonly ILogManager _logger;

        public FolderController(IWebHostEnvironment environment, IFolderRepository folders, IUserPermissions userPermissions, ITenantResolver tenants, ILogManager logger)
        {
            _environment = environment;
            _folders = folders;
            _userPermissions = userPermissions;
            _tenants = tenants;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Folder> Get(string siteid)
        {
            List<Folder> folders = new List<Folder>();
            foreach (Folder folder in _folders.GetFolders(int.Parse(siteid)))
            {
                if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.Permissions))
                {
                    folders.Add(folder);
                }
            }
            return folders;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Folder Get(int id)
        {
            Folder folder = _folders.GetFolder(id);
            if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.Permissions))
            {
                return folder;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Folder {Folder}", folder);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        [HttpGet("{siteId}/{path}")]
        public Folder GetByPath(int siteId, string path)
        {
            var folderPath = WebUtility.UrlDecode(path);
            Folder folder = _folders.GetFolder(siteId, folderPath);
            if (folder != null)
                if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.Permissions))
                {
                    return folder;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Folder {Folder}",
                        folder);
                    HttpContext.Response.StatusCode = 401;
                    return null;
                }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "Folder not found {path}",
                    path);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Folder Post([FromBody] Folder folder)
        {
            if (ModelState.IsValid)
            {
                string permissions;
                if (folder.ParentId != null)
                {
                    permissions = _folders.GetFolder(folder.ParentId.Value).Permissions;
                }
                else
                {
                    permissions = new List<Permission> {
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true),
                    }.EncodePermissions();
                }
                if (_userPermissions.IsAuthorized(User, PermissionNames.Edit, permissions))
                {
                    if (folder.IsPathValid())
                    {
                        if (string.IsNullOrEmpty(folder.Path) && folder.ParentId != null)
                        {
                            Folder parent = _folders.GetFolder(folder.ParentId.Value);
                            folder.Path = Utilities.PathCombine(parent.Path, folder.Name);
                        }
                        folder.Path = Utilities.PathCombine(folder.Path, Path.DirectorySeparatorChar.ToString());
                        folder = _folders.AddFolder(folder);
                        _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Added {Folder}", folder);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Name Not Valid {Folder}", folder);
                        HttpContext.Response.StatusCode = 401;
                        folder = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Folder {Folder}", folder);
                    HttpContext.Response.StatusCode = 401;
                    folder = null;
                }
            }
            return folder;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Folder Put(int id, [FromBody] Folder folder)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Folder, folder.FolderId, PermissionNames.Edit))
            {
                if (folder.IsPathValid())
                {
                    if (folder.ParentId != null)
                    {
                        Folder parent = _folders.GetFolder(folder.ParentId.Value);
                        folder.Path = Utilities.PathCombine(parent.Path, folder.Name);
                    }
                    folder.Path = Utilities.PathCombine(folder.Path, Path.DirectorySeparatorChar.ToString());

                    Models.Folder _folder = _folders.GetFolder(id, false);
                    if (_folder.Path != folder.Path && Directory.Exists(GetFolderPath(_folder)))
                    {
                        Directory.Move(GetFolderPath(_folder), GetFolderPath(folder));
                    }

                    folder = _folders.UpdateFolder(folder);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Updated {Folder}", folder);
                }
                else
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Name Not Valid {Folder}", folder);
                    HttpContext.Response.StatusCode = 401;
                    folder = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Folder {Folder}", folder);
                HttpContext.Response.StatusCode = 401;
                folder = null;
            }
            return folder;
        }

        // PUT api/<controller>/?siteid=x&folderid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = RoleNames.Registered)]
        public void Put(int siteid, int folderid, int? parentid)
        {
            if (_userPermissions.IsAuthorized(User, EntityNames.Folder, folderid, PermissionNames.Edit))
            {
                int order = 1;
                List<Folder> folders = _folders.GetFolders(siteid).ToList();
                foreach (Folder folder in folders.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
                {
                    if (folder.Order != order)
                    {
                        folder.Order = order;
                        _folders.UpdateFolder(folder);
                    }
                    order += 2;
                }
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Order Updated {SiteId} {FolderId} {ParentId}", siteid, folderid, parentid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Folder Order {SiteId} {FolderId} {ParentId}", siteid, folderid, parentid);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            if (_userPermissions.IsAuthorized(User, EntityNames.Folder, id, PermissionNames.Edit))
            {
                Models.Folder _folder = _folders.GetFolder(id, false);
                if (Directory.Exists(GetFolderPath(_folder)))
                {
                    Directory.Delete(GetFolderPath(_folder));
                }
                _folders.DeleteFolder(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Folder Deleted {FolderId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Folder {FolderId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }

        private string GetFolderPath(Folder folder)
        {
            return Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", _tenants.GetTenant().TenantId.ToString(), "Sites", folder.SiteId.ToString(), folder.Path);
        }
    }
}
