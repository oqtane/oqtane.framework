using Microsoft.AspNetCore.Authorization;

namespace Oqtane.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string EntityName { get; }

        public string PermissionName { get; }

        public string Roles { get; } // semi-colon delimited

        public PermissionRequirement(string entityName, string permissionName, string roles)
        {
            EntityName = entityName;
            PermissionName = permissionName;
            Roles = roles;
        }
    }
}
