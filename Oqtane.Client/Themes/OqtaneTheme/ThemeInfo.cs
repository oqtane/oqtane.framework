using Oqtane.Models;

namespace Oqtane.Themes.OqtaneTheme
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "Oqtane Theme",
            Version = "1.0.0"
        };
    }
}
