using System.Collections.Generic;

namespace Oqtane.Modules.[Module]s
{
    public class Module : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "[Module]" },
                    { "Description", "[Module]" },
                    { "Version", "1.0.0" },
                    { "ServerAssemblyName", "Oqtane.Server" }
                };
                return properties;
            }
        }
    }
}
