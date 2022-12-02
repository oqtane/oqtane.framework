using Microsoft.AspNetCore.Authorization;

namespace Oqtane.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string EntityName { get; }

        public string PermissionName { get; }

        public string Roles { get; }

        public bool RequireEntityId { get; }

        public PermissionRequirement(string entityName, string permissionName, string roles, bool requireEntityId)
        {
            EntityName = entityName;
            PermissionName = permissionName;
            Roles = roles;
            RequireEntityId = requireEntityId;
        }
    }
}
