using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Oqtane.Infrastructure
{
    public interface IConfigManager
    {
        public IConfigurationSection GetSection(string sectionKey);
        public T GetSetting<T>(string settingKey, T defaultValue);
        public T GetSetting<T>(string sectionKey, string settingKey, T defaultValue);
        public Dictionary<string, string> GetSettings(string sectionKey);
        void AddOrUpdateSetting<T>(string key, T value, bool reload);
        void AddOrUpdateSetting<T>(string file, string key, T value, bool reload);
        void RemoveSetting(string key, bool reload);
        void RemoveSetting(string file, string key, bool reload);
        void Reload();

        public string GetConnectionString();
        public string GetConnectionString(string name);
        public bool IsInstalled();
        public string GetInstallationId();
    }
}
