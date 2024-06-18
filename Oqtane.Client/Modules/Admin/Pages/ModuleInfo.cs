using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Pages
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Pages",
            Description = "Manage Pages",
            Categories = "Admin",
            Version = Constants.Version,
            ServerManagerType = "Oqtane.Modules.Admin.Pages.PagesManager, Oqtane.Server",
            PermissionNames = $"{PermissionNames.View},{PermissionNames.Edit}," +
                $"{EntityNames.Page}:{PermissionNames.Write}:{RoleNames.Admin}"
        };
    }
}
