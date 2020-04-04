using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IThemeService
    {
        Task<List<Theme>> GetThemesAsync();
        Dictionary<string, string> GetThemeTypes(List<Theme> themes);
        Dictionary<string, string> GetPaneLayoutTypes(List<Theme> themes, string themeName);
        Dictionary<string, string> GetContainerTypes(List<Theme> themes);
        Task InstallThemesAsync();
        Task DeleteThemeAsync(string themeName);
    }
}
