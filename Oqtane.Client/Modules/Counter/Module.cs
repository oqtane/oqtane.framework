using Oqtane.Models;

namespace Oqtane.Modules.Counter
{
    public class Module : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Counter",
            Description = "Increments a counter",
            Version = "1.0.0"
        };
    }
}
