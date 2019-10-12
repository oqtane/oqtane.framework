using System.Collections.Generic;

namespace Oqtane.Themes.BlazorTheme
{
    public class Theme : ITheme
    {
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "Name", "Blazor Theme" },
                    { "Version", "1.0.0" }
                };
                return properties;
            }
        }
    }
}
