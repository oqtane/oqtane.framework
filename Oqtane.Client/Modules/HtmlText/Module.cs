using Oqtane.Modules;
using System.Collections.Generic;

namespace Oqtane.Client.Modules.HtmlText
{
    public class Module : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "HtmlText" },
                    { "Description", "Renders HTML or Text" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
