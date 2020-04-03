using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
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
