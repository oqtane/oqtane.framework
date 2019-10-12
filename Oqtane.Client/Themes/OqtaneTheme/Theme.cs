using System.Collections.Generic;

namespace Oqtane.Themes.OqtaneTheme
{
    public class Theme : ITheme
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Oqtane Theme" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
