using System.Collections.Generic;

namespace Oqtane.Themes.Theme2
{
    public class Theme : ITheme
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Theme2" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
