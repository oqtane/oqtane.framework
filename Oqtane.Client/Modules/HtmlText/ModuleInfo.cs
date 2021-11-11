using Oqtane.Documentation;
using Oqtane.Models;

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
            SettingsType = "Oqtane.Modules.HtmlText.Settings, Oqtane.Client"
        };
    }
}
