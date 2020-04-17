using Oqtane.Models;
using Oqtane.Modules;

namespace [Owner].[Module]s.Modules
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "[Module]",
            Description = "[Module]",
            Version = "1.0.0",
            Dependencies = "[Owner].[Module]s.Module.Shared",
            ServerManagerType = "[ServerManagerType]"
        };
    }
}
