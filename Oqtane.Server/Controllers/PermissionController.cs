using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PermissionController : Controller
    {
        private readonly IPermissionRepository Permissions;
        private readonly ILogManager logger;

        public PermissionController(IPermissionRepository Permissions, ILogManager logger)
        {
            this.Permissions = Permissions;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Permission> Get(string entityname, int entityid, string permissionname)
        {
            return Permissions.GetPermissions(entityname, entityid, permissionname);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Permission Get(int id)
        {
            return Permissions.GetPermission(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Permission Post([FromBody] Permission Permission)
        {
            if (ModelState.IsValid)
            {
                Permission = Permissions.AddPermission(Permission);
                logger.AddLog(this.GetType().FullName, LogLevel.Information, "Permission Added {Permission}", Permission);
            }
            return Permission;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Permission Put(int id, [FromBody] Permission Permission)
        {
            if (ModelState.IsValid)
            {
                Permission = Permissions.UpdatePermission(Permission);
                logger.AddLog(this.GetType().FullName, LogLevel.Information, "Permission Updated {Permission}", Permission);
            }
            return Permission;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Permissions.DeletePermission(id);
            logger.AddLog(this.GetType().FullName, LogLevel.Information, "Permission Deleted {PermissionId}", id);
        }
    }
}
