using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IThemeService
    {
        Task<List<Theme>> GetThemesAsync();
        Dictionary<string, string> CalculateSelectableThemes(List<Theme> themes);
        Dictionary<string, string> CalculateSelectablePaneLayouts(List<Theme> themes);
        Dictionary<string, string> CalculateSelectableContainers(List<Theme> themes);
    }
}
