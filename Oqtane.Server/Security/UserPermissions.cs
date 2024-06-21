using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using System.Linq;
using System.Security.Claims;
using Oqtane.Repository;
using Oqtane.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public interface IUserPermissions
    {
        bool IsAuthorized(ClaimsPrincipal user, int siteId, string entityName, int entityId, string permissionName, string roles);
        bool IsAuthorized(ClaimsPrincipal user, int siteId, string entityName, int entityId, string permissionName);
        bool IsAuthorized(ClaimsPrincipal user, string permissionName, List<Permission> permissions);
        bool IsAuthorized(ClaimsPrincipal user, string permissionName, string permissions);
        User GetUser(ClaimsPrincipal user);
        User GetUser();

        [Obsolete("IsAuthorized(ClaimsPrincipal principal, string entityName, int entityId, string permissionName) is deprecated. Use IsAuthorized(ClaimsPrincipal principal, int siteId, string entityName, int entityId, string permissionName) instead.", false)]
        bool IsAuthorized(ClaimsPrincipal user, string entityName, int entityId, string permissionName);
    }

    public class UserPermissions : IUserPermissions
    {
        private readonly IPermissionRepository _permissions;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUserRoleRepository _userRoleRepository;
        public UserPermissions(IPermissionRepository permissions, IUserRoleRepository userRoleRepository, IHttpContextAccessor accessor)
        {
            _permissions = permissions;
            _userRoleRepository = userRoleRepository;
            _accessor = accessor;
        }

        public bool IsAuthorized(ClaimsPrincipal principal, int siteId, string entityName, int entityId, string permissionName, string roles)
        {
            var permissions = _permissions.GetPermissions(siteId, entityName, entityId, permissionName).ToList();
            if (permissions != null && permissions.Count != 0)
            {
                return IsAuthorized(principal, permissionName, permissions.ToList());
            }
            else
            {
                return UserSecurity.IsAuthorized(GetUser(principal), roles);
            }
        }

        public bool IsAuthorized(ClaimsPrincipal principal, int siteId, string entityName, int entityId, string permissionName)
        {
            return IsAuthorized(principal, permissionName, _permissions.GetPermissions(siteId, entityName, entityId, permissionName).ToList());
        }

        public bool IsAuthorized(ClaimsPrincipal principal, string permissionName, List<Permission> permissionList)
        {
            return UserSecurity.IsAuthorized(GetUser(principal), permissionName, permissionList);
        }

        public User GetUser(ClaimsPrincipal principal)
        {
            User user = new User();
            user.IsAuthenticated = false;
            user.Username = "";
            user.UserId = -1;
            user.Roles = "";

            if (principal == null) return user;

            user.IsAuthenticated = principal.Identity.IsAuthenticated;
            if (user.IsAuthenticated)
            {                
                user.Username = principal.Identity.Name;
                if (principal.Claims.Any(item => item.Type == ClaimTypes.NameIdentifier))
                {
                    user.UserId = int.Parse(principal.Claims.First(item => item.Type == ClaimTypes.NameIdentifier).Value);
                }
                
                var userroles = _userRoleRepository.GetUserRoles(user.UserId, principal.SiteId());
                foreach (var claim in principal.Claims.Where(item => item.Type == ClaimTypes.Role))
                {
                    //var isEffective = _userRoleRepository.GetUserRole(user.UserId, userroles.First(item => item.Role.Name == claim.Value).RoleId);
                    var matchedRole = userroles.FirstOrDefault(item => item.Role.Name == claim.Value);
                    if (matchedRole != null)
                    {
                        if (Utilities.IsRoleEffective(matchedRole.EffectiveDate, matchedRole.ExpiryDate))
                        {
                            user.Roles += claim.Value + ";";
                        }
                    }
                    else
                    {
                        user.Roles += claim.Value + ";";
                    }
                }
                if (user.Roles != "") user.Roles = ";" + user.Roles;
            }
            return user;
        }

        public User GetUser()
        {
            if (_accessor.HttpContext != null)
            {
                return GetUser(_accessor.HttpContext.User);
            }
            else
            {
                return null;
            }
        }

        // deprecated
        public bool IsAuthorized(ClaimsPrincipal principal, string entityName, int entityId, string permissionName)
        {
            return IsAuthorized(principal, permissionName, _permissions.GetPermissions(_accessor.HttpContext.GetAlias().SiteId, entityName, entityId, permissionName).ToList());
        }

        [Obsolete("IsAuthorized(ClaimsPrincipal principal, string permissionName, string permissions) is deprecated. Use IsAuthorized(ClaimsPrincipal principal, string permissionName, List<Permission> permissionList) instead", false)]
        public bool IsAuthorized(ClaimsPrincipal principal, string permissionName, string permissions)
        {
            return UserSecurity.IsAuthorized(GetUser(principal), permissionName, JsonSerializer.Deserialize<List<Permission>>(permissions));
        }
    }
}
