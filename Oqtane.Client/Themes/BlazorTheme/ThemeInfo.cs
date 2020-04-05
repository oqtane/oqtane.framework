using Oqtane.Models;

namespace Oqtane.Themes.BlazorTheme
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "Blazor Theme",
            Version = "1.0.0"
        };

    }
}
