using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Application.MyModule
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MyModule",
            Description = "Example module",
            Version = "1.0.0",
            ServerManagerType = "Oqtane.Application.Manager.MyModuleManager, Oqtane.Application.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Oqtane.Application.Shared.Oqtane",
            PackageName = "Oqtane.Application" 
        };
    }
}
