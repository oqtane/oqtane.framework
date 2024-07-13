using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.Files
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "File Management",
            Description = "File Management",
            Version = Constants.Version,
            Categories = "Admin",
            ServerManagerType = "Oqtane.Modules.Admin.Files.Manager.FileManager, Oqtane.Server"
        };
    }
}
