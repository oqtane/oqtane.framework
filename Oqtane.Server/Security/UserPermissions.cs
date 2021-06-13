using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using System.Linq;
using System.Security.Claims;
using Oqtane.Repository;

namespace Oqtane.Security
{
    public class UserPermissions : IUserPermissions
    {
        private readonly IPermissionRepository _permissions;
        private readonly IHttpContextAccessor _accessor;

        public UserPermissions(IPermissionRepository permissions, IHttpContextAccessor accessor)
        {
            _permissions = permissions;
            _accessor = accessor;
        }

        public bool IsAuthorized(ClaimsPrincipal principal, string entityName, int entityId, string permissionName)
        {
            return IsAuthorized(principal, permissionName, _permissions.GetPermissionString(entityName, entityId, permissionName));
        }

        public bool IsAuthorized(ClaimsPrincipal principal, string permissionName, string permissions)
        {
            return UserSecurity.IsAuthorized(GetUser(principal), permissionName, permissions);
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
                if (principal.Claims.Any(item => item.Type == ClaimTypes.PrimarySid))
                {
                    user.UserId = int.Parse(principal.Claims.First(item => item.Type == ClaimTypes.PrimarySid).Value);
                }
                foreach (var claim in principal.Claims.Where(item => item.Type == ClaimTypes.Role))
                {
                    user.Roles += claim.Value + ";";
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
    }
}
