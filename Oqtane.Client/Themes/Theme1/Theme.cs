using System.Collections.Generic;

namespace Oqtane.Themes.Theme1
{
    public class Theme : ITheme
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Theme1" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
