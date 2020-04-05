using Oqtane.Models;

namespace Oqtane.Modules.HtmlText
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "HtmlText",
            Description = "Renders HTML or Text",
            Version = "1.0.0",
            ServerAssemblyName = "Oqtane.Server"
        };
    }
}
