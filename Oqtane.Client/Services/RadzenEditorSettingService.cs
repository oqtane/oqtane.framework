using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Modules.Controls;
using Oqtane.Services;

namespace Oqtane.Services
{
    public interface IRadzenEditorSettingService
    {
        Task<int> GetSettingScopeAsync(int moduleId);

        Task UpdateSettingScopeAsync(int moduleId, int scope);

        Task<RadzenEditorSetting> LoadSettingsFromModuleAsync(int moduleId);

        Task<RadzenEditorSetting> LoadSettingsFromSiteAsync(int siteId);

        Task SaveSiteSettingsAsync(int siteId, RadzenEditorSetting editorSetting);

        Task SaveModuleSettingsAsync(int moduleId, RadzenEditorSetting editorSetting);
    }
    public class RadzenEditorSettingService : IRadzenEditorSettingService, IService
    {
        private const string SettingPrefix = "rzeditor:";

        private readonly ISettingService _settingService;

        public RadzenEditorSettingService(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<int> GetSettingScopeAsync(int moduleId)
        {
            var key = $"{SettingPrefix}Scope";
            var settings = await _settingService.GetModuleSettingsAsync(moduleId);
            if (settings.ContainsKey(key) && int.TryParse(settings[key], out int value))
            {
                return value;
            }

            return 0; // site as default
        }

        public async Task UpdateSettingScopeAsync(int moduleId, int scope)
        {
            var settings = new Dictionary<string, string>
            {
                { $"{SettingPrefix}Scope", scope.ToString() }
            };

            await _settingService.UpdateModuleSettingsAsync(settings, moduleId);
        }

        public async Task<RadzenEditorSetting> LoadSettingsFromModuleAsync(int moduleId)
        {
            var settings = await _settingService.GetModuleSettingsAsync(moduleId);
            return ReadSettings(settings);
        }

        public async Task<RadzenEditorSetting> LoadSettingsFromSiteAsync(int siteId)
        {
            var settings = await _settingService.GetSiteSettingsAsync(siteId);
            return ReadSettings(settings);
        }

        public async Task SaveSiteSettingsAsync(int siteId, RadzenEditorSetting editorSetting)
        {
            var settings = CreateSettingsDictionary(editorSetting);
            if (settings.Any())
            {
                await _settingService.UpdateSiteSettingsAsync(settings, siteId);
            }
        }

        public async Task SaveModuleSettingsAsync(int moduleId, RadzenEditorSetting editorSetting)
        {
            var settings = CreateSettingsDictionary(editorSetting);
            if (settings.Any())
            {
                await _settingService.UpdateModuleSettingsAsync(settings, moduleId);
            }
        }

        private RadzenEditorSetting ReadSettings(IDictionary<string, string> settings)
        {
            var setting = new RadzenEditorSetting
            {
                Theme = RadzenEditorDefinitions.DefaultTheme,
                Background = RadzenEditorDefinitions.DefaultBackground,
                ToolbarItems = RadzenEditorDefinitions.DefaultToolbarItems
            };

            if (settings != null)
            {
                var themeKey = $"{SettingPrefix}Theme";
                var backgroundKey = $"{SettingPrefix}Background";
                var toolbarItemsKey = $"{SettingPrefix}ToolbarItems";

                if (settings.ContainsKey(themeKey) && !string.IsNullOrEmpty(settings[themeKey]))
                {
                    setting.Theme = settings[themeKey];
                }

                if (settings.ContainsKey(backgroundKey) && !string.IsNullOrEmpty(settings[backgroundKey]))
                {
                    setting.Background = settings[backgroundKey];
                }

                if (settings.ContainsKey(toolbarItemsKey) && !string.IsNullOrEmpty(settings[toolbarItemsKey]))
                {
                    setting.ToolbarItems = settings[toolbarItemsKey];
                }
            }

            return setting;
        }

        private Dictionary<string, string> CreateSettingsDictionary(RadzenEditorSetting editorSetting)
        {
            var settings = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(editorSetting.Theme))
            {
                settings.Add($"{SettingPrefix}Theme", editorSetting.Theme);
            }
            if (!string.IsNullOrEmpty(editorSetting.Background))
            {
                settings.Add($"{SettingPrefix}Background", editorSetting.Background);
            }
            if (!string.IsNullOrEmpty(editorSetting.ToolbarItems))
            {
                settings.Add($"{SettingPrefix}ToolbarItems", editorSetting.ToolbarItems);
            }

            return settings;
        }
    }
}
