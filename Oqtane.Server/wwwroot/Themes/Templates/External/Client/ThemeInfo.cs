using Oqtane.Models;
using Oqtane.Themes;

namespace [Owner].[Theme]
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "[Theme]",
            Version = "1.0.0",
            PackageName = "[Owner].[Theme]"
        };

    }
}
