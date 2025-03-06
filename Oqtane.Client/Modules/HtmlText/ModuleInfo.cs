using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "HtmlText",
            Description = "Renders HTML or Text Content",
            Version = "1.0.1",
            ServerManagerType = "Oqtane.Modules.HtmlText.Manager.HtmlTextManager, Oqtane.Server",
            ReleaseVersions = "1.0.0,1.0.1",
            SettingsType = "Oqtane.Modules.HtmlText.Settings, Oqtane.Client",
            Resources = new List<Resource>()
            {
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Module.css" }
            }
        };
    }
}
