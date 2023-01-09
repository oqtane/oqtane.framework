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
using Oqtane.Modules.Admin.Roles;

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
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId && (userid != null || rolename != null))
            {
                var userroles = _userRoles.GetUserRoles(SiteId).ToList();
                if (userid != null)
                {
                    int UserId = int.TryParse(userid, out UserId) ? UserId : -1;
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
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public UserRole Get(int id)
        {
            var userrole = _userRoles.GetUserRole(id);
            if (userrole != null && SiteValid(userrole.Role.SiteId))
            {
                return Filter(userrole, _userPermissions.GetUser().UserId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Role Get Attempt {UserRoleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        private UserRole Filter(UserRole userrole, int userid)
        {
            if (userrole != null)
            {
                userrole.User.Password = "";
                userrole.User.IsAuthenticated = false;
                userrole.User.TwoFactorCode = "";
                userrole.User.TwoFactorExpiry = null;

                if (!_userPermissions.IsAuthorized(User, userrole.User.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) && userid != userrole.User.UserId)
                {
                    userrole.User.Email = "";
                    userrole.User.PhotoFileId = null;
                    userrole.User.LastLoginOn = DateTime.MinValue;
                    userrole.User.LastIPAddress = "";
                    userrole.User.Roles = "";
                    userrole.User.CreatedBy = "";
                    userrole.User.CreatedOn = DateTime.MinValue;
                    userrole.User.ModifiedBy = "";
                    userrole.User.ModifiedOn = DateTime.MinValue;
                    userrole.User.DeletedBy = "";
                    userrole.User.DeletedOn = DateTime.MinValue;
                    userrole.User.IsDeleted = false;
                    userrole.User.TwoFactorRequired = false;
                }
            }
            return userrole;
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
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UserRole, userRole.UserRoleId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userRole);

                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userRole.UserId, SyncEventActions.Refresh);
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
            if (ModelState.IsValid && role != null && SiteValid(role.SiteId) && RoleValid(role.Name) && _userRoles.GetUserRole(userRole.UserRoleId, false) != null)
            {
                userRole = _userRoles.UpdateUserRole(userRole);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UserRole, userRole.UserRoleId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userRole.UserId, SyncEventActions.Refresh);
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
            UserRole userrole = _userRoles.GetUserRole(id);
            if (userrole != null && SiteValid(userrole.Role.SiteId) && RoleValid(userrole.Role.Name))
            {
                _userRoles.DeleteUserRole(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UserRole, userrole.UserRoleId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRole}", userrole);

                if (userrole.Role.Name == RoleNames.Host)
                {
                    // add site specific user roles to preserve user access
                    var role = _roles.GetRoles(_alias.SiteId).FirstOrDefault(item => item.Name == RoleNames.Registered);
                    userrole = _userRoles.AddUserRole(new UserRole { UserId = userrole.UserId, RoleId = role.RoleId, EffectiveDate = null, ExpiryDate = null });
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userrole);
                    role = _roles.GetRoles(_alias.SiteId).FirstOrDefault(item => item.Name == RoleNames.Admin);
                    userrole = _userRoles.AddUserRole(new UserRole { UserId = userrole.UserId, RoleId = role.RoleId, EffectiveDate = null, ExpiryDate = null });
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", userrole);
                }

                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.User, userrole.UserId, SyncEventActions.Refresh);
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
