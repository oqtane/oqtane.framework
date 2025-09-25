using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Theme"/> entries
    /// </summary>
    public interface IThemeService
    {

        /// <summary>
        /// Returns a list of available themes
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Theme>> GetThemesAsync(int siteId);

        /// <summary>
        /// Returns a specific theme
        /// </summary>
        /// <param name="themeId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<Theme> GetThemeAsync(int themeId, int siteId);

        /// <summary>
        /// Returns a theme <see cref="ThemeControl"/>s containing a specific theme control type
        /// </summary>
        /// <param name="themes"></param>
        /// <param name="themeControlType"></param>
        /// <returns></returns>
        Theme GetTheme(List<Theme> themes, string themeControlType);

        /// <summary>
        /// Returns a list of <see cref="ThemeControl"/>s from the given themes
        /// </summary>
        /// <param name="themes"></param>
        /// <returns></returns>
        List<ThemeControl> GetThemeControls(List<Theme> themes);

        /// <summary>
        /// Returns a list of <see cref="ThemeControl"/>s for a theme containing a specific theme control type
        /// </summary>
        /// <param name="themes"></param>
        /// <param name="themeControlType"></param>
        /// <returns></returns>
        List<ThemeControl> GetThemeControls(List<Theme> themes, string themeControlType);

        /// <summary>
        /// Returns a list of containers (<see cref="ThemeControl"/>) for a theme containing a specific theme control type
        /// </summary>
        /// <param name="themes"></param>
        /// <param name="themeControlType"></param>
        /// <returns></returns>
        List<ThemeControl> GetContainerControls(List<Theme> themes, string themeControlType);

        /// <summary>
        /// Updates a existing theme
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        Task UpdateThemeAsync(Theme theme);

        /// <summary>
        /// Deletes a theme
        /// </summary>
        /// <param name="themeId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task DeleteThemeAsync(int themeId, int siteId);

        /// <summary>
        /// Creates a new theme
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        Task<Theme> CreateThemeAsync(Theme theme);

        /// <summary>
        /// Returns a list of theme templates (<see cref="Template"/>)
        /// </summary>
        /// <returns></returns>
        Task<List<Template>> GetThemeTemplatesAsync();


        /// <summary>
        /// Returns a list of layouts (<see cref="ThemeControl"/>) from the given themes with a matching theme name
        /// </summary>
        /// <param name="themes"></param>
        /// <param name="themeName"></param>
        /// <returns></returns>
        List<ThemeControl> GetLayoutControls(List<Theme> themes, string themeName);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ThemeService : ServiceBase, IThemeService
    {
        public ThemeService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Theme");

        public async Task<List<Theme>> GetThemesAsync(int siteId)
        {
            List<Theme> themes = await GetJsonAsync<List<Theme>>($"{ApiUrl}?siteid={siteId}");
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

        public async Task DeleteThemeAsync(int themeId, int siteId)
        {
            await DeleteAsync($"{ApiUrl}/{themeId}?siteid={siteId}");
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
