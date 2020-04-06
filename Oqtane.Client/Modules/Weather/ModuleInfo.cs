using Oqtane.Models;

namespace Oqtane.Modules.Weather
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Weather",
            Description = "Displays random weather using a service",
            Version = "1.0.0"
        };
    }
}
