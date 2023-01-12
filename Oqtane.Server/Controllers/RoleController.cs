using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roles;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public RoleController(IRoleRepository roles, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _roles = roles;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&global=true/false
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Role> Get(string siteid, string global)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                if (string.IsNullOrEmpty(global))
                {
                    global = "False";
                }
                return _roles.GetRoles(SiteId, bool.Parse(global));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Role Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Role Get(int id)
        {
            var role = _roles.GetRole(id);
            if (role != null && (role.SiteId == _alias.SiteId || User.IsInRole(RoleNames.Host)))
            {
                return role;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Role Get Attempt {RoleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = $"{EntityNames.Role}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public Role Post([FromBody] Role role)
        {
            if (ModelState.IsValid && role.SiteId == _alias.SiteId)
            {
                role = _roles.AddRole(role);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Role, role.RoleId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Role Added {Role}", role);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Role Post Attempt {Role}", role);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                role = null;
            }
            return role;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = $"{EntityNames.Role}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public Role Put(int id, [FromBody] Role role)
        {
            if (ModelState.IsValid && role.SiteId == _alias.SiteId && _roles.GetRole(role.RoleId, false) != null)
            {
                role = _roles.UpdateRole(role);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Role, role.RoleId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Role Updated {Role}", role);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Role Put Attempt {Role}", role);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                role = null;
            }
            return role;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = $"{EntityNames.Role}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public void Delete(int id)
        {
            var role = _roles.GetRole(id);
            if (role != null && !role.IsSystem && role.SiteId == _alias.SiteId)
            {
                _roles.DeleteRole(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Role, role.RoleId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Role Deleted {RoleId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Role Delete Attempt {RoleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
