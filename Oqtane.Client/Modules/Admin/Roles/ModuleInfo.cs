using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Roles
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Roles",
            Description = "Manage Roles",
            Categories = "Admin",
            Version = Constants.Version,
            PermissionNames = $"{PermissionNames.View},{PermissionNames.Edit}," +
                $"{EntityNames.Role}:{PermissionNames.Write}:{RoleNames.Admin}," +
                $"{EntityNames.UserRole}:{PermissionNames.Write}:{RoleNames.Admin}"
        };
    }
}
