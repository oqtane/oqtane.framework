using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleRepository _userRoles;
        private readonly IRoleRepository _roles;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public UserRoleController(IUserRoleRepository userRoles, IRoleRepository roles, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _userRoles = userRoles;
            _roles = roles;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<UserRole> Get(string siteid)
        {
            return _userRoles.GetUserRoles(int.Parse(siteid));
        }
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public UserRole Get(int id)
        {
            return _userRoles.GetUserRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public UserRole Post([FromBody] UserRole userRole)
        {
            var role = _roles.GetRole(userRole.RoleId);
            if (ModelState.IsValid && (User.IsInRole(RoleNames.Host) || role.Name != RoleNames.Host))
            {
                if (role.Name == RoleNames.Host)
                {
                    // host roles can only exist at global level - remove all site specific user roles
                    _userRoles.DeleteUserRoles(userRole.UserId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Roles Deleted For UserId {UserId}", userRole.UserId);
                }

                userRole = _userRoles.AddUserRole(userRole);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);

                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userRole.UserId);
            }
            return userRole;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public UserRole Put(int id, [FromBody] UserRole userRole)
        {
            var role = _roles.GetRole(userRole.RoleId);
            if (ModelState.IsValid && (User.IsInRole(RoleNames.Host) || role.Name != RoleNames.Host))
            {
                userRole = _userRoles.UpdateUserRole(userRole);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userRole.UserId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Role Updated {UserRole}", userRole);
            }
            return userRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            UserRole userRole = _userRoles.GetUserRole(id);
            if (User.IsInRole(RoleNames.Host) || userRole.Role.Name != RoleNames.Host)
            {
                _userRoles.DeleteUserRole(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRole}", userRole);

                if (userRole.Role.Name == RoleNames.Host)
                {
                    // add site specific user roles to preserve user access
                    var role = _roles.GetRoles(_alias.SiteId).FirstOrDefault(item => item.Name == RoleNames.Registered);
                    userRole = _userRoles.AddUserRole(new UserRole { UserId = userRole.UserId, RoleId = role.RoleId, EffectiveDate = null, ExpiryDate = null });
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);
                    role = _roles.GetRoles(_alias.SiteId).FirstOrDefault(item => item.Name == RoleNames.Admin);
                    userRole = _userRoles.AddUserRole(new UserRole { UserId = userRole.UserId, RoleId = role.RoleId, EffectiveDate = null, ExpiryDate = null });
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);
                }

                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userRole.UserId);
            }
        }
    }
}
