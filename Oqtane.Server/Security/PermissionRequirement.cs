using Microsoft.AspNetCore.Authorization;

namespace Oqtane.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string EntityName { get; }

        public string PermissionName { get; }

        public PermissionRequirement(string EntityName, string PermissionName)
        {
            this.EntityName = EntityName;
            this.PermissionName = PermissionName;
        }
    }
}
