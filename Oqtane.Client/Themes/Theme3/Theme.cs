using System.Collections.Generic;

namespace Oqtane.Themes.Theme3
{
    public class Theme : ITheme
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Theme3" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
