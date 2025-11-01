using Oqtane.Models;
using Oqtane.Modules;

namespace [Owner].Module.[Module]
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "[Module]",
            Description = "[Description]",
            Version = "1.0.0",
            ServerManagerType = "[ServerManagerType]"
        };
    }
}
