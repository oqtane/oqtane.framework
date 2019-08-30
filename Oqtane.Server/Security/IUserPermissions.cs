using System.Security.Claims;

namespace Oqtane.Security
{
    public interface IUserPermissions
    {
        bool IsAuthorized(ClaimsPrincipal User, string EntityName, int EntityId, string PermissionName);
    }
}
