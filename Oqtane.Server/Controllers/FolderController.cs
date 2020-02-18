using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Infrastructure;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class FolderController : Controller
    {
        private readonly IFolderRepository Folders;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public FolderController(IFolderRepository Folders, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.Folders = Folders;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Folder> Get(string siteid)
        {
            List<Folder> folders = new List<Folder>();
            foreach(Folder folder in Folders.GetFolders(int.Parse(siteid)))
            {
                if (UserPermissions.IsAuthorized(User, "Browse", folder.Permissions))
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
            Folder folder = Folders.GetFolder(id);
            if (UserPermissions.IsAuthorized(User, "Browse", folder.Permissions))
            {
                return folder;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Folder {Folder}", folder);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Folder Post([FromBody] Folder Folder)
        {
            if (ModelState.IsValid)
            {
                string permissions;
                if (Folder.ParentId != null)
                {
                    permissions = Folders.GetFolder(Folder.ParentId.Value).Permissions;
                }
                else
                {
                    permissions = UserSecurity.SetPermissionStrings(new List<PermissionString> { new PermissionString { PermissionName = "Edit", Permissions = Constants.AdminRole } });
                }
                if (UserPermissions.IsAuthorized(User, "Edit", permissions))
                {
                    if (string.IsNullOrEmpty(Folder.Path) && Folder.ParentId != null)
                    {
                        Folder parent = Folders.GetFolder(Folder.ParentId.Value);
                        Folder.Path = parent.Path + Folder.Name + "\\";
                    }
                    Folder = Folders.AddFolder(Folder);
                    logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Added {Folder}", Folder);
                }
                else
                {
                    logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Folder {Folder}", Folder);
                    HttpContext.Response.StatusCode = 401;
                    Folder = null;
                }
            }
            return Folder;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Folder Put(int id, [FromBody] Folder Folder)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Folder", Folder.FolderId, "Edit"))
            {
                if (string.IsNullOrEmpty(Folder.Path) && Folder.ParentId != null)
                {
                    Folder parent = Folders.GetFolder(Folder.ParentId.Value);
                    Folder.Path = parent.Path + Folder.Name + "\\";
                }
                Folder = Folders.UpdateFolder(Folder);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Updated {Folder}", Folder);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Folder {Folder}", Folder);
                HttpContext.Response.StatusCode = 401;
                Folder = null;
            }
            return Folder;
        }

        // PUT api/<controller>/?siteid=x&folderid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int siteid, int folderid, int? parentid)
        {
            if (UserPermissions.IsAuthorized(User, "Folder", folderid, "Edit"))
            {
                int order = 1;
                List<Folder> folders = Folders.GetFolders(siteid).ToList();
                foreach (Folder folder in folders.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
                {
                    if (folder.Order != order)
                    {
                        folder.Order = order;
                        Folders.UpdateFolder(folder);
                    }
                    order += 2;
                }
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Order Updated {SiteId} {FolderId} {ParentId}", siteid, folderid, parentid);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Folder Order {SiteId} {FolderId} {ParentId}", siteid, folderid, parentid);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            if (UserPermissions.IsAuthorized(User, "Folder", id, "Edit"))
            {
                Folders.DeleteFolder(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Folder Deleted {FolderId}", id);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Folder {FolderId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }
    }
}
