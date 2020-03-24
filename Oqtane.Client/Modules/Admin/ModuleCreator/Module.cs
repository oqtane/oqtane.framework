using System.Collections.Generic;

namespace Oqtane.Modules.Admin.ModuleCreator
{
    public class Module : IModule
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Module Creator" },
                    { "Description", "Enables software developers to quickly create modules by automating many of the initial module creation tasks" },
                    { "Version", "1.0.0" },
                    { "Categories", "Developer" }
                };
                return properties;
            }
        }
    }
}
