using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System.Collections.Generic;

namespace [Owner].[Module]
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "[Module]",
            Description = "[Description]",
            Version = "1.0.0",
            ServerManagerType = "[ServerManagerType]",
            ReleaseVersions = "1.0.0",
            Dependencies = "[Owner].[Module].Shared.Oqtane",
            PackageName = "[Owner].[Module]",
            Resources = new List<Resource>()
            {
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Module.css" },
                new Resource { ResourceType = ResourceType.Script, Url = "~/Module.js" }
            }			
        };
    }
}
