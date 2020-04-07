using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Modules.[Module]s
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "[Module]",
            Description = "[Module]",
            Version = "1.0.0",
            ServerAssemblyName = "Oqtane.Server"
        };
    }
}
