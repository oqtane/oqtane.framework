using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IThemeRepository
    {
        IEnumerable<Theme> GetThemes();
        Theme GetTheme(int themeId, int siteId);
        void UpdateTheme(Theme theme);
        void DeleteTheme(int themeId);
        List<Theme> FilterThemes(List<Theme> themes);
    }
}
