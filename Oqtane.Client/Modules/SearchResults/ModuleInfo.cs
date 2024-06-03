using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.SearchResults
{
    [PrivateApi("Mark SearchResults classes as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Search Results",
            Description = "Display Search Results",
            Version = "1.0.0",
            ServerManagerType = "",
            ReleaseVersions = "1.0.0",
            SettingsType = "Oqtane.Modules.SearchResults.Settings, Oqtane.Client",
            Resources = new List<Resource>()
            {
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Module.css" }
            }
        };
    }
}
