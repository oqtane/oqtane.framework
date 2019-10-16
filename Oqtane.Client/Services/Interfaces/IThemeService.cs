using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IThemeService
    {
        Task<List<Theme>> GetThemesAsync();
        Dictionary<string, string> GetThemeTypes(List<Theme> Themes);
        Dictionary<string, string> GetPaneLayoutTypes(List<Theme> Themes, string ThemeName);
        Dictionary<string, string> GetContainerTypes(List<Theme> Themes);
        Task InstallThemesAsync();
        Task DeleteThemeAsync(string ThemeName);
    }
}
