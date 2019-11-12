using System.Collections.Generic;

namespace Oqtane.Modules.HtmlText
{
    public class ModuleInfo : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "HtmlText" },
                    { "Description", "Renders HTML or Text" },
                    { "Version", "1.0.0" },
                    { "ServerAssemblyName", "Oqtane.Server" }
                };
                return properties;
            }
        }
    }
}
