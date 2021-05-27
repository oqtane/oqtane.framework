using Microsoft.Extensions.Configuration;

namespace Oqtane.Infrastructure
{
    public interface IConfigManager
    {
        public IConfigurationSection GetSection(string sectionKey);
        public string GetSetting(string sectionKey, string settingKey, string defaultValue);
        void AddOrUpdateSetting<T>(string key, T value, bool reload);
        void AddOrUpdateSetting<T>(string file, string key, T value, bool reload);
        void RemoveSetting(string key, bool reload);
        void RemoveSetting(string file, string key, bool reload);
        void Reload();
    }
}
