using Oqtane.Models;
using Oqtane.Repository;
using System.Linq;
using System.Security.Claims;

namespace Oqtane.Security
{
    public class UserPermissions : IUserPermissions
    {
        private readonly IPermissionRepository Permissions;

        public UserPermissions(IPermissionRepository Permissions)
        {
            this.Permissions = Permissions;
        }

        public bool IsAuthorized(ClaimsPrincipal User, string EntityName, int EntityId, string PermissionName)
        {
            return IsAuthorized(User, PermissionName, Permissions.EncodePermissions(EntityId, Permissions.GetPermissions(EntityName, EntityId, PermissionName).ToList()));
        }

        public bool IsAuthorized(ClaimsPrincipal User, string PermissionName, string Permissions)
        {
            User user = new User();
            user.UserId = -1;
            user.Roles = "";

            if (User != null)
            {
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

            return UserSecurity.IsAuthorized(user, PermissionName, Permissions);
        }
    }
}
