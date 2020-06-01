using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ThemeService : ServiceBase, IThemeService
    {
        private readonly HttpClient _http;

        public ThemeService(HttpClient http) : base(http)
        {
            _http = http;
        }

        private string Apiurl => CreateApiUrl("Theme");

        public async Task<List<Theme>> GetThemesAsync()
        {
            List<Theme> themes = await GetJsonAsync<List<Theme>>(Apiurl);
            return themes.OrderBy(item => item.Name).ToList();
        }

        public List<ThemeControl> GetThemeControls(List<Theme> themes)
        {
            return themes.SelectMany(item => item.Themes).ToList();
        }

        public List<ThemeControl> GetLayoutControls(List<Theme> themes, string themeName)
        {
            return themes.Where(item => Utilities.GetTypeName(themeName).StartsWith(Utilities.GetTypeName(item.ThemeName)))
                .SelectMany(item => item.Layouts).ToList();
        }

        public List<ThemeControl> GetContainerControls(List<Theme> themes, string themeName)
        {
            return themes.Where(item => Utilities.GetTypeName(themeName).StartsWith(Utilities.GetTypeName(item.ThemeName)))
                .SelectMany(item => item.Containers).ToList();
        }

        public async Task InstallThemesAsync()
        {
            await GetJsonAsync<List<string>>($"{Apiurl}/install");
        }

        public async Task DeleteThemeAsync(string themeName)
        {
            await DeleteAsync($"{Apiurl}/{themeName}");
        }
    }
}
