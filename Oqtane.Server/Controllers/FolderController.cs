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
            if (siteid == "")
            {
                return Folders.GetFolders();
            }
            else
            {
                return Folders.GetFolders(int.Parse(siteid));
            }
        }

        // GET api/<controller>/5?userid=x
        [HttpGet("{id}")]
        public Folder Get(int id)
        {
            return Folders.GetFolder(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Folder Post([FromBody] Folder Folder)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Edit", Folder.Permissions))
            {
                Folder = Folders.AddFolder(Folder);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Folder Added {Folder}", Folder);
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
                Folder = Folders.UpdateFolder(Folder);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Folder Updated {Folder}", Folder);
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
        }
    }
}
