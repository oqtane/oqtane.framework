using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Modules.Counter
{
    public class Module : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Counter" },
                    { "Description", "Increments a counter" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }

        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Counter",
            Description = "Increments a counter",
            Version = "1.0.0"
        };
    }
}
