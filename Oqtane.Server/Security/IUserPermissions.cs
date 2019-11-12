using System.Security.Claims;

namespace Oqtane.Security
{
    public interface IUserPermissions
    {
        bool IsAuthorized(ClaimsPrincipal User, string EntityName, int EntityId, string PermissionName);
        bool IsAuthorized(ClaimsPrincipal User, string PermissionName, string Permissions);
    }
}
