using Oqtane.Models;
using System.Security.Claims;

namespace Oqtane.Security
{
    public interface IUserPermissions
    {
        bool IsAuthorized(ClaimsPrincipal user, string entityName, int entityId, string permissionName);
        bool IsAuthorized(ClaimsPrincipal user, string permissionName, string permissions);
        User GetUser(ClaimsPrincipal user);
        User GetUser();
    }
}
