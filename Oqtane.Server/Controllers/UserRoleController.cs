using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Linq;
using System.Net;
using Oqtane.Security;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleRepository _userRoles;
        private readonly IRoleRepository _roles;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public UserRoleController(IUserRoleRepository userRoles, IRoleRepository roles, IUserPermissions userPermissions, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _userRoles = userRoles;
            _roles = roles;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&userid=y&rolename=z
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<UserRole> Get(string siteid, string userid = null, string rolename = null)
        {
            int SiteId;
            int UserId = -1;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId && (userid != null && int.TryParse(userid, out UserId) || rolename != null))
            {
                if (IsAuthorized(UserId, rolename, SiteId))
                {
                    var userroles = _userRoles.GetUserRoles(SiteId).ToList();
                    if (UserId != -1)
                    {
                        userroles = userroles.Where(item => item.UserId == UserId).ToList();
                    }
                    if (rolename != null)
                    {
                        userroles = userroles.Where(item => item.Role.Name == rolename).ToList();
                    }
                    var user = _userPermissions.GetUser();
                    for (int i = 0; i < userroles.Count(); i++)
                    {
                        userroles[i] = Filter(userroles[i], user.UserId);
                    }
                    return userroles.OrderBy(u => u.User.DisplayName);

                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UserRole Get Attempt For Site {SiteId} User {UserId} Role {RoleName}", siteid, userid, rolename);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UserRole Get Attempt For Site {SiteId} User {UserId} Role {RoleName}", siteid, userid, rolename);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public UserRole Get(int id)
        {
            var userrole = _userRoles.GetUserRole(id);
            if (userrole != null && SiteValid(userrole.Role.SiteId) && IsAuthorized(userrole.UserId, userrole.Role.Name, userrole.Role.SiteId ?? -1))
            {
                return Filter(userrole, _userPermissions.GetUser().UserId);
            }
            else
            {
                if (userrole != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Role Get Attempt {UserRoleId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        private bool IsAuthorized(int userId, string roleName, int siteId)
        {
            bool authorized = true;
            if (userId != -1)
            {
                authorized = (_userPermissions.GetUser(User).UserId == userId);
            }
            if (authorized && !string.IsNullOrEmpty(roleName))
            {
                authorized = User.IsInRole(roleName);
            }
            if (!authorized)
            {
                authorized = _userPermissions.IsAuthorized(User, siteId, EntityNames.UserRole, -1, PermissionNames.Write, RoleNames.Admin);
            }
            return authorized;
        }

        private UserRole Filter(UserRole userrole, int userid)
        {
            // include all properties if authorized
            if (_userPermissions.IsAuthorized(User, userrole.User.SiteId, EntityNames.UserRole, -1, PermissionNames.Write, RoleNames.Admin))
            {
                return userrole;
            }
            else
            {
                // clone object to avoid mutating cache 
                UserRole filtered = null;

                if (userrole != null)
                {
                    filtered = new UserRole();

                    // include public properties
                    filtered.UserRoleId = userrole.UserRoleId;
                    filtered.UserId = userrole.UserId;
                    filtered.RoleId = userrole.RoleId;

                    filtered.User = new User();
                    filtered.User.SiteId = userrole.User.SiteId;
                    filtered.User.UserId = userrole.User.UserId;
                    filtered.User.Username = userrole.User.Username;
                    filtered.User.DisplayName = userrole.User.DisplayName;

                    filtered.Role = new Role();
                    filtered.Role.SiteId = userrole.Role.SiteId;
                    filtered.Role.RoleId = userrole.Role.RoleId;
                    filtered.Role.Name = userrole.Role.Name;
                }

                return filtered;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = $"{EntityNames.UserRole}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public UserRole Post([FromBody] UserRole userRole)
        {
            var role = _roles.GetRole(userRole.RoleId);
            if (ModelState.IsValid && role != null && SiteValid(role.SiteId) && RoleValid(role.Name))
            {
                userRole = _userRoles.AddUserRole(userRole);
                _syncManager.AddSyncEvent(_alias, EntityNames.UserRole, userRole.UserRoleId, SyncEventActions.Create);
                _syncManager.AddSyncEvent(_alias, EntityNames.User, userRole.UserId, SyncEventActions.Reload);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UserRole Post Attempt {UserRole}", userRole);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                userRole = null;
            }
            return userRole;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = $"{EntityNames.UserRole}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public UserRole Put(int id, [FromBody] UserRole userRole)
        {
            var role = _roles.GetRole(userRole.RoleId);
            if (ModelState.IsValid && role != null && SiteValid(role.SiteId) && RoleValid(role.Name) && userRole.UserRoleId == id && _userRoles.GetUserRole(userRole.UserRoleId, false) != null)
            {
                userRole = _userRoles.UpdateUserRole(userRole);
                _syncManager.AddSyncEvent(_alias, EntityNames.UserRole, userRole.UserRoleId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias, EntityNames.User, userRole.UserId, SyncEventActions.Reload);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Role Updated {UserRole}", userRole);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Role Put Attempt {UserRole}", userRole);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                userRole = null;
            }
            return userRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = $"{EntityNames.UserRole}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public void Delete(int id)
        {
            UserRole userRole = _userRoles.GetUserRole(id);
            if (userRole != null && SiteValid(userRole.Role.SiteId) && RoleValid(userRole.Role.Name))
            {
                _userRoles.DeleteUserRole(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.UserRole, userRole.UserRoleId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias, EntityNames.User, userRole.UserId, SyncEventActions.Reload);
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
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Role Delete Attempt {UserRoleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private bool SiteValid(int? SiteId)
        {
            return (SiteId == _alias.SiteId || (SiteId == null && User.IsInRole(RoleNames.Host)));
        }

        private bool RoleValid(string RoleName)
        {
            return (RoleName != RoleNames.Host || User.IsInRole(RoleNames.Host));
        }
    }
}
