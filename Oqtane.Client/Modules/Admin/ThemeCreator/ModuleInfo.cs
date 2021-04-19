using Oqtane.Models;

namespace Oqtane.Modules.Admin.ThemeCreator
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Theme Creator",
            Description = "Enables software developers to quickly create themes by automating many of the initial theme creation tasks",
            Version = "1.0.0",
            Categories = "Developer"
        };
    }
}
