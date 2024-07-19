using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Admin.SearchResults
{
    [PrivateApi("Mark this as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Search Results",
            Description = "Search Results",
            Categories = "Admin",
            Version = Constants.Version,
            SettingsType = "Oqtane.Modules.Admin.SearchResults.Settings, Oqtane.Client"
        };
    }
}
