using Oqtane.Documentation;
using Oqtane.Models;

namespace Oqtane.Modules.Admin.ModuleCreator
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
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
