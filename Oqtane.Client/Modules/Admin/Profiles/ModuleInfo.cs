using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Profiles
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Profiles",
            Description = "Manage Profiles",
            Categories = "Admin",
            Version = Constants.Version,
            PermissionNames = $"{PermissionNames.View},{PermissionNames.Edit}," +
                $"{EntityNames.Profile}:{PermissionNames.Write}:{RoleNames.Admin}"
        };
    }
}
