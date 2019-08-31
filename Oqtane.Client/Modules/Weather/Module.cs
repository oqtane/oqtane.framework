using Oqtane.Modules;
using System.Collections.Generic;

namespace Oqtane.Client.Modules.Weather
{
    public class Module : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Weather" },
                    { "Description", "Displays random weather using a service" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
