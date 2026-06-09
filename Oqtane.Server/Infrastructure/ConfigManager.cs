using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oqtane.Shared;

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

    public class ConfigManager : IConfigManager
    {
        private readonly IConfigurationRoot _config;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ConfigManager> _filelogger;

        private static string appsettingsPath = "";
        private static string appsettingsFilename = "";

        public ConfigManager(IConfigurationRoot config, IWebHostEnvironment environment, ILogger<ConfigManager> filelogger)
        {
            _config = config;
            _environment = environment;
            _filelogger = filelogger;

            appsettingsPath = GetAppSettingsOverridePath();
            if (!string.IsNullOrEmpty(appsettingsPath))
            {
                appsettingsFilename = GetAppSettingsOverrideFilename();
            }
            if (string.IsNullOrEmpty(appsettingsFilename))
            {
                appsettingsPath = Directory.GetCurrentDirectory();
                appsettingsFilename = $"appsettings.{_environment.EnvironmentName}.json";
                if (_environment.EnvironmentName == "Development" || !File.Exists(Path.Combine(appsettingsPath, appsettingsFilename)))
                {
                    appsettingsFilename = "appsettings.json"; // fallback
                }
            }
        }

        public static string GetAppSettingsOverridePath()
        {
            var appSettingsOverrideFilePath = Environment.GetEnvironmentVariable(Constants.AppSettingsOverrideEnvironmentVariable);
            if (!string.IsNullOrEmpty(appSettingsOverrideFilePath) && Directory.Exists(appSettingsOverrideFilePath))
            {
                return appSettingsOverrideFilePath;
            }
            return null;
        }

        public static string GetAppSettingsOverrideFilename()
        {
            var appSettingsOverrideFilePath = GetAppSettingsOverridePath();
            if (!string.IsNullOrEmpty(appSettingsOverrideFilePath))
            {
                var appSettingsOverrideFileName = Path.Combine(appSettingsOverrideFilePath, "appsettings.override.json");
                return File.Exists(appSettingsOverrideFileName) ? appSettingsOverrideFileName : null;
            }
            return null;
        }

        public IConfigurationSection GetSection(string key)
        {
            return _config.GetSection(key);
        }

        public T GetSetting<T>(string sectionKey, T defaultValue)
        {
            return GetSetting(sectionKey, "", defaultValue);
        }

        public T GetSetting<T>(string sectionKey, string settingKey, T defaultValue)
        {
            T value;
            if (!string.IsNullOrEmpty(settingKey))
            {
               value = _config.GetSection(sectionKey).GetValue(settingKey, defaultValue);
            }
            else
            {
                value = _config.GetValue(sectionKey, defaultValue);
            }
            if (value == null) value = defaultValue;
            return value;
        }

        public Dictionary<string, string> GetSettings(string sectionKey)
        {
            var settings = new Dictionary<string, string>();
            foreach (var kvp in _config.GetSection(sectionKey).GetChildren().AsEnumerable())
            {
                settings.Add(kvp.Key, kvp.Value);
            }
            return settings;
        }

        public void AddOrUpdateSetting<T>(string key, T value, bool reload)
        {
            AddOrUpdateSetting(appsettingsFilename, key, value, reload);
        }

        public void AddOrUpdateSetting<T>(string file, string key, T value, bool reload)
        {
            try
            {
                var path = Path.Combine(appsettingsPath, file);
                JsonNode node = JsonNode.Parse(File.ReadAllText(path));
                SetValueRecursively(node, key, value);
                File.WriteAllText(path, JsonSerializer.Serialize(node, new JsonSerializerOptions() { WriteIndented = true }));
                if (reload) Reload();
            }
            catch (Exception ex)
            {
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error Updating App Setting {key} - {ex}"));
            }
        }

        public void RemoveSetting(string key, bool reload)
        {
            RemoveSetting(appsettingsFilename, key, reload);
        }

        public void RemoveSetting(string file, string key, bool reload)
        {
            try
            {
                var path = Path.Combine(appsettingsPath, file);
                JsonNode node = JsonNode.Parse(File.ReadAllText(path));
                RemovePropertyRecursively(node, key);
                File.WriteAllText(path, JsonSerializer.Serialize(node, new JsonSerializerOptions() { WriteIndented = true }));
                if (reload) Reload();
            }
            catch (Exception ex)
            {
                _filelogger.LogError(Utilities.LogMessage(this, $"Oqtane Error: Error Removing App Setting {key} - {ex}"));
            }
        }

        private void SetValueRecursively<T>(JsonNode json, string key, T value)
        {
            if (json != null && key != null && value != null)
            {
                var remainingSections = key.Split(":", 2);

                var currentSection = remainingSections[0];
                if (remainingSections.Length > 1)
                {
                    var nextSection = remainingSections[1];
                    SetValueRecursively(json[currentSection] ??= new JsonObject(), nextSection, value);
                }
                else
                {
                    if (value.GetType() == typeof(string) && (value.ToString()!.StartsWith("[") || value.ToString()!.StartsWith("{")))
                    {
                        json[currentSection] = JsonNode.Parse(value.ToString()!);
                    }
                    else
                    {
                        json[currentSection] = JsonValue.Create(value);
                    }
                }
            }
        }

        private void RemovePropertyRecursively(JsonNode json, string key)
        {
            if (json != null && key != null)
            {
                var remainingSections = key.Split(":", 2);

                var currentSection = remainingSections[0];
                if (remainingSections.Length > 1)
                {
                    var nextSection = remainingSections[1];
                    if (json[currentSection] != null)
                    {
                        RemovePropertyRecursively(json[currentSection], nextSection);
                    }
                }
                else
                {
                    if (json.AsObject().ContainsKey(currentSection))
                    {
                        json.AsObject().Remove(currentSection);
                    }
                }
            }
        }

        public void Reload()
        {
            _config.Reload();
        }

        public string GetConnectionString()
        {
            return _config.GetConnectionString(SettingKeys.ConnectionStringKey);
        }

        public string GetConnectionString(string name)
        {
            return _config.GetConnectionString(name);
        }

        public bool IsInstalled()
        {
            return !string.IsNullOrEmpty(GetConnectionString());
        }

        public string GetInstallationId()
        {
            var installationid = GetSetting("InstallationId", "");
            if (installationid == "")
            {
                installationid = Guid.NewGuid().ToString();
                AddOrUpdateSetting("InstallationId", installationid, true);
            }
            var version = GetSetting("InstallationVersion", "");
            if (version != Constants.Version)
            {
                AddOrUpdateSetting("InstallationVersion", Constants.Version, true);
                AddOrUpdateSetting("InstallationDate", DateTime.UtcNow.ToString("yyyyMMddHHmm"), true);
            }
            return installationid;
        }
    }
}
