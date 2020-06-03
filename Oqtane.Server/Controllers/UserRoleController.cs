using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleRepository _userRoles;
        private readonly ITenantResolver _tenants;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public UserRoleController(IUserRoleRepository userRoles, ITenantResolver tenants, ISyncManager syncManager, ILogManager logger)
        {
            _userRoles = userRoles;
            _syncManager = syncManager;
            _tenants = tenants;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public IEnumerable<UserRole> Get(string siteid)
        {
            return _userRoles.GetUserRoles(int.Parse(siteid));
        }
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Get(int id)
        {
            return _userRoles.GetUserRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Post([FromBody] UserRole userRole)
        {
            if (ModelState.IsValid)
            {
                userRole = _userRoles.AddUserRole(userRole);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.User, userRole.UserId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);
            }
            return userRole;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Put(int id, [FromBody] UserRole userRole)
        {
            if (ModelState.IsValid)
            {
                userRole = _userRoles.UpdateUserRole(userRole);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.User, userRole.UserId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Role Updated {UserRole}", userRole);
            }
            return userRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            UserRole userRole = _userRoles.GetUserRole(id);
            _userRoles.DeleteUserRole(id);
            _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.User, userRole.UserId);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRole}", userRole);
        }
    }
}
