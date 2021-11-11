using Oqtane.Documentation;
using Oqtane.Models;

namespace Oqtane.Themes.BlazorTheme
{
    [PrivateApi("Mark Build-In Theme-Info classes as private, since it's not very useful in the public docs")]
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "Blazor Theme",
            Version = "1.0.0"
        };

    }
}
