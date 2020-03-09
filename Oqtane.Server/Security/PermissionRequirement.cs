using Microsoft.AspNetCore.Authorization;

namespace Oqtane.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string EntityName { get; }

        public string PermissionName { get; }

        public PermissionRequirement(string entityName, string permissionName)
        {
            EntityName = entityName;
            PermissionName = permissionName;
        }
    }
}
