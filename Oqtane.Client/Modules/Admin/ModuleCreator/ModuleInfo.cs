using Oqtane.Models;

namespace Oqtane.Modules.Admin.ModuleCreator
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Module Creator",
            Description = "Enables software developers to quickly create modules by automating many of the initial module creation tasks",
            Version = "1.0.0",
            Categories = "Developer"
        };
    }
}
