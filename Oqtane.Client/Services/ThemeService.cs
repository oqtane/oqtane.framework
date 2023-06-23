using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ThemeService : ServiceBase, IThemeService
    {
        public ThemeService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Theme");

        public async Task<List<Theme>> GetThemesAsync()
        {
            List<Theme> themes = await GetJsonAsync<List<Theme>>(ApiUrl);
            return themes.OrderBy(item => item.Name).ToList();
        }
        public async Task<Theme> GetThemeAsync(int themeId, int siteId)
        {
            return await GetJsonAsync<Theme>($"{ApiUrl}/{themeId}?siteid={siteId}");
        }

        public Theme GetTheme(List<Theme> themes, string themeControlType)
        {
            return themes.FirstOrDefault(item => item.Themes.Any(item => item.TypeName == themeControlType));
        }

        public List<ThemeControl> GetThemeControls(List<Theme> themes)
        {
            return themes.SelectMany(item => item.Themes).OrderBy(item => item.Name).ToList();
        }

        public List<ThemeControl> GetThemeControls(List<Theme> themes, string themeControlType)
        {
            return GetTheme(themes, themeControlType)?.Themes.OrderBy(item => item.Name).ToList();
        }


        public List<ThemeControl> GetContainerControls(List<Theme> themes, string themeControlType)
        {
            return GetTheme(themes, themeControlType)?.Containers.OrderBy(item => item.Name).ToList();
        }

        public async Task UpdateThemeAsync(Theme theme)
        {
            await PutJsonAsync($"{ApiUrl}/{theme.ThemeId}", theme);
        }

        public async Task DeleteThemeAsync(string themeName)
        {
            await DeleteAsync($"{ApiUrl}/{themeName}");
        }

        public async Task<Theme> CreateThemeAsync(Theme theme)
        {
            return await PostJsonAsync($"{ApiUrl}", theme);
        }

        public async Task<List<Template>> GetThemeTemplatesAsync()
        {
            List<Template> templates = await GetJsonAsync<List<Template>>($"{ApiUrl}/templates");
            return templates;
        }

        //[Obsolete("This method is deprecated.", false)]
        public List<ThemeControl> GetLayoutControls(List<Theme> themes, string themeName)
        {
            return null;
        }
    }
}
