using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Oqtane.Infrastructure
{
    public class ConfigManager : IConfigManager
    {
        private readonly IConfigurationRoot _config;

        public ConfigManager(IConfigurationRoot config)
        {
            _config = config;
        }

        public IConfigurationSection GetSection(string key)
        {
            return _config.GetSection(key);
        }

        public string GetSetting(string sectionKey, string settingKey, string defaultValue)
        {
            var value = _config.GetSection(sectionKey).GetValue(settingKey, defaultValue);
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }

        public void AddOrUpdateSetting<T>(string key, T value, bool reload)
        {
            AddOrUpdateSetting("appsettings.json", key, value, reload);
        }

        public void AddOrUpdateSetting<T>(string file, string key, T value, bool reload)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), file);
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(path));
                SetValueRecursively(key, jsonObj, value, "set");
                File.WriteAllText(path, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
                if (reload) Reload();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error modifying app settings {0}", ex);
            }
        }

        public void RemoveSetting(string key, bool reload)
        {
            RemoveSetting("appsettings.json", key, reload);
        }

        public void RemoveSetting(string file, string key, bool reload)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), file);
                dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(path));
                SetValueRecursively(key, jsonObj, "", "remove");
                File.WriteAllText(path, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
                if (reload) Reload();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error modifying app settings {0}", ex);
            }
        }

        private void SetValueRecursively<T>(string key, dynamic jsonObj, T value, string action)
        {
            var remainingSections = key.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                var nextSection = remainingSections[1];
                jsonObj[currentSection] ??= new JObject();
                SetValueRecursively(nextSection, jsonObj[currentSection], value, action);
            }
            else
            {
                switch (action)
                {
                    case "set":
                        jsonObj[currentSection] = value;
                        break;
                    case "remove":
                        jsonObj.Property(currentSection).Remove();
                        break;
                }
            }
        }

        public void Reload()
        {
            _config.Reload();
        }
    }
}
