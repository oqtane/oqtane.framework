using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Modules.Weather
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

        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Weather",
            Description = "Displays random weather using a service",
            Version = "1.0.0"
        };
    }
}
