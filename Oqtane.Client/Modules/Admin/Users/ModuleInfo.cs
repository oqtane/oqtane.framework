using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Users
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Users",
            Description = "Manage Users",
            Categories = "Admin",
            Version = Constants.Version,
            PermissionNames = $"{PermissionNames.View},{PermissionNames.Edit}," +
                $"{EntityNames.User}:{PermissionNames.Write}:{RoleNames.Admin}," +
                $"{EntityNames.UserRole}:{PermissionNames.Write}:{RoleNames.Admin}"
        };
    }
}
