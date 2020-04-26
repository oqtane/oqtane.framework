using Oqtane.Models;

namespace Oqtane.Modules.HtmlText
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "HtmlText",
            Description = "Renders HTML or Text Content",
            Version = "1.0.0",
            ServerManagerType = "Oqtane.Modules.HtmlText.Manager.HtmlTextManager, Oqtane.Server",
            ReleaseVersions = "1.0.0"
        };
    }
}
