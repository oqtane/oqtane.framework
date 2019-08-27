using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PermissionController : Controller
    {
        private readonly IPermissionRepository Permissions;

        public PermissionController(IPermissionRepository Permissions)
        {
            this.Permissions = Permissions;
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
        [Authorize(Roles = "Administrators")]
        public Permission Post([FromBody] Permission Permission)
        {
            if (ModelState.IsValid)
            {
                Permission = Permissions.AddPermission(Permission);
            }
            return Permission;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrators")]
        public Permission Put(int id, [FromBody] Permission Permission)
        {
            if (ModelState.IsValid)
            {
                Permission = Permissions.UpdatePermission(Permission);
            }
            return Permission;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrators")]
        public void Delete(int id)
        {
            Permissions.DeletePermission(id);
        }
    }
}
