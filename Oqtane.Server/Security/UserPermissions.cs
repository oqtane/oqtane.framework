using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using Oqtane.Repository;
using System.Linq;
using System.Security.Claims;

namespace Oqtane.Security
{
    public class UserPermissions : IUserPermissions
    {
        private readonly IPermissionRepository Permissions;
        private readonly IHttpContextAccessor Accessor;

        public UserPermissions(IPermissionRepository Permissions, IHttpContextAccessor Accessor)
        {
            this.Permissions = Permissions;
            this.Accessor = Accessor;
        }

        public bool IsAuthorized(ClaimsPrincipal User, string EntityName, int EntityId, string PermissionName)
        {
            return IsAuthorized(User, PermissionName, Permissions.EncodePermissions(EntityId, Permissions.GetPermissions(EntityName, EntityId, PermissionName).ToList()));
        }

        public bool IsAuthorized(ClaimsPrincipal User, string PermissionName, string Permissions)
        {
            return UserSecurity.IsAuthorized(GetUser(User), PermissionName, Permissions);
        }

        public User GetUser(ClaimsPrincipal User)
        {
            User user = new User();
            user.Username = "";
            user.IsAuthenticated = false;
            user.UserId = -1;
            user.Roles = "";

            if (User != null)
            {
                user.Username = User.Identity.Name;
                user.IsAuthenticated = User.Identity.IsAuthenticated;
                var idclaim = User.Claims.Where(item => item.Type == ClaimTypes.PrimarySid).FirstOrDefault();
                if (idclaim != null)
                {
                    user.UserId = int.Parse(idclaim.Value);
                    foreach (var claim in User.Claims.Where(item => item.Type == ClaimTypes.Role))
                    {
                        user.Roles += claim.Value + ";";
                    }
                    if (user.Roles != "") user.Roles = ";" + user.Roles;
                }
            }

            return user;
        }

        public User GetUser()
        {
            return GetUser(Accessor.HttpContext.User);
        }
    }
}
