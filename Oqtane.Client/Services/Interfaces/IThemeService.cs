using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IThemeService
    {
        Task<List<Theme>> GetThemesAsync();
        List<ThemeControl> GetThemeControls(List<Theme> themes);
        List<ThemeControl> GetLayoutControls(List<Theme> themes, string themeName);
        List<ThemeControl> GetContainerControls(List<Theme> themes, string themeName);
        Task InstallThemesAsync();
        Task DeleteThemeAsync(string themeName);
    }
}
