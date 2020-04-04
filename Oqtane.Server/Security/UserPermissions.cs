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

        public bool IsAuthorized(ClaimsPrincipal user, string entityName, int entityId, string permissionName)
        {
            return IsAuthorized(user, permissionName, _permissions.EncodePermissions(_permissions.GetPermissions(entityName, entityId, permissionName).ToList()));
        }

        public bool IsAuthorized(ClaimsPrincipal user, string permissionName, string permissions)
        {
            return UserSecurity.IsAuthorized(GetUser(user), permissionName, permissions);
        }

        public User GetUser(ClaimsPrincipal user)
        {
            User resultUser = new User();
            resultUser.Username = "";
            resultUser.IsAuthenticated = false;
            resultUser.UserId = -1;
            resultUser.Roles = "";

            if (user == null) return resultUser;

            resultUser.Username = user.Identity.Name;
            resultUser.IsAuthenticated = user.Identity.IsAuthenticated;
            var idclaim = user.Claims.FirstOrDefault(item => item.Type == ClaimTypes.PrimarySid);
            if (idclaim != null)
            {
                resultUser.UserId = int.Parse(idclaim.Value);
                foreach (var claim in user.Claims.Where(item => item.Type == ClaimTypes.Role))
                {
                    resultUser.Roles += claim.Value + ";";
                }

                if (resultUser.Roles != "") resultUser.Roles = ";" + resultUser.Roles;
            }
            return resultUser;
        }

        public User GetUser()
        {
            return GetUser(_accessor.HttpContext.User);
        }
    }
}
