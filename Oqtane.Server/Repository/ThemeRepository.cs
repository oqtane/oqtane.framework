using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Security;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Themes;
using System.Reflection.Metadata;
using Oqtane.Migrations.Master;

namespace Oqtane.Repository
{
    public class ThemeRepository : IThemeRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;
        private readonly ITenantManager _tenants;
        private readonly ISettingRepository _settings;
        private readonly IServerStateManager _serverState;
        private readonly string settingprefix = "SiteEnabled:";

        public ThemeRepository(MasterDBContext context, IMemoryCache cache, ITenantManager tenants, ISettingRepository settings, IServerStateManager serverState)
        {
            _db = context;
            _cache = cache;
            _tenants = tenants;
            _settings = settings;
            _serverState = serverState;
        }

        public IEnumerable<Theme> GetThemes()
        {
            // for consistency siteid should be passed in as parameter, but this would require breaking change
            return LoadThemes(_tenants.GetAlias().SiteId);
        }

        public Theme GetTheme(int themeId, int siteId)
        {
            List<Theme> themes = LoadThemes(siteId);
            return themes.Find(item => item.ThemeId == themeId);
        }

        public void UpdateTheme(Theme theme)
        {
            _db.Entry(theme).State = EntityState.Modified;
            _db.SaveChanges();

            var settingname = $"{settingprefix}{_tenants.GetAlias().SiteKey}";
            var setting = _settings.GetSetting(EntityNames.Theme, theme.ThemeId, settingname);
            if (setting == null)
            {
                _settings.AddSetting(new Setting { EntityName = EntityNames.Theme, EntityId = theme.ThemeId, SettingName = settingname, SettingValue = theme.IsEnabled.ToString(), IsPrivate = true });
            }
            else
            {
                setting.SettingValue = theme.IsEnabled.ToString();
                _settings.UpdateSetting(setting);
            }

            _cache.Remove($"themes:{_tenants.GetAlias().SiteKey}");
        }

        public void DeleteTheme(int themeId)
        {
            Theme theme = _db.Theme.Find(themeId);
            _settings.DeleteSettings(EntityNames.Theme, themeId);
            _db.Theme.Remove(theme);
            _db.SaveChanges();
            _cache.Remove($"themes:{_tenants.GetAlias().SiteKey}");
        }

        public List<Theme> FilterThemes(List<Theme> themes)
        {
            var Themes = new List<Theme>();

            foreach (Theme theme in themes.Where(item => item.IsEnabled))
            {
                var Theme = new Theme();
                Theme.ThemeName = theme.ThemeName;
                Theme.Name = theme.Name;
                Theme.Resources = theme.Resources;
                Theme.Themes = theme.Themes;
                Theme.Containers = theme.Containers;
                Theme.ThemeSettingsType = theme.ThemeSettingsType;
                Theme.ContainerSettingsType = theme.ContainerSettingsType;
                Theme.PackageName = theme.PackageName;
                Themes.Add(Theme);
            }

            return Themes;
        }

        private List<Theme> LoadThemes(int siteId)
        {
            // get themes
            List<Theme> themes = _cache.GetOrCreate($"themes:{_tenants.GetAlias().SiteKey}", entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return ProcessThemes(siteId);
            });

            return themes;
        }

        private List<Theme> ProcessThemes(int siteId)
        {
            // get themes
            List<Theme> Themes = LoadThemesFromAssemblies();

            // get themes in database
            List<Theme> themes = _db.Theme.ToList();

            // sync theme assemblies with database
            foreach (Theme Theme in Themes)
            {
                Theme theme = themes.Where(item => item.ThemeName == Theme.ThemeName).FirstOrDefault();
                if (theme == null)
                {
                    // new theme
                    theme = new Theme { ThemeName = Theme.ThemeName, Version = Theme.Version };
                    _db.Theme.Add(theme);
                    _db.SaveChanges();
                }
                else
                {
                    // override user customizable property values
                    Theme.Name = (!string.IsNullOrEmpty(theme.Name)) ? theme.Name : Theme.Name;

                    // remove theme from list as it is already synced
                    themes.Remove(theme);
                }

                // format theme control names
                foreach (var themecontrol in Theme.Themes)
                {
                    themecontrol.Name = Theme.Name + " - " + themecontrol.Name;
                }

                // load db properties
                Theme.ThemeId = theme.ThemeId;
                Theme.CreatedBy = theme.CreatedBy;
                Theme.CreatedOn = theme.CreatedOn;
                Theme.ModifiedBy = theme.ModifiedBy;
                Theme.ModifiedOn = theme.ModifiedOn;
            }

            // any remaining themes are orphans
            foreach (Theme theme in themes)
            {
                _db.Theme.Remove(theme); // delete
                _db.SaveChanges();
            }

            if (siteId != -1)
            {
                var siteKey = _tenants.GetAlias().SiteKey;

                // get settings for site
                var settings = _settings.GetSettings(EntityNames.Theme).ToList();

                // populate theme site settings
                var serverState = _serverState.GetServerState(siteKey);
                foreach (Theme theme in Themes)
                {
                    theme.SiteId = siteId;

                    var setting = settings.FirstOrDefault(item => item.EntityId == theme.ThemeId && item.SettingName == $"{settingprefix}{_tenants.GetAlias().SiteKey}");
                    if (setting != null)
                    {
                        theme.IsEnabled = bool.Parse(setting.SettingValue);
                    }
                    else
                    {
                        theme.IsEnabled = theme.IsAutoEnabled;
                    }

                    if (theme.IsEnabled)
                    {
                        // build list of assemblies for site
                        if (!serverState.Assemblies.Contains(theme.AssemblyName))
                        {
                            serverState.Assemblies.Add(theme.AssemblyName);
                        }
                        if (!string.IsNullOrEmpty(theme.Dependencies))
                        {
                            foreach (var assembly in theme.Dependencies.Replace(".dll", "").Split(',', StringSplitOptions.RemoveEmptyEntries).Reverse())
                            {
                                if (!serverState.Assemblies.Contains(assembly.Trim()))
                                {
                                    serverState.Assemblies.Insert(0, assembly.Trim());
                                }
                            }
                        }
                        // build list of scripts for site
                        if (theme.Resources != null)
                        {
                            foreach (var resource in theme.Resources.Where(item => item.Level == ResourceLevel.Site))
                            {
                                if (!serverState.Scripts.Contains(resource))
                                {
                                    serverState.Scripts.Add(resource);
                                }
                            }
                        }
                    }
                }
            }

            return Themes;
        }

        private List<Theme> LoadThemesFromAssemblies()
        {
            List<Theme> themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Utilities.GetTypeName(assembly.FullName) + ".dll")))
                {
                    themes = LoadThemesFromAssembly(themes, assembly);
                }
            }

            return themes;
        }

        private List<Theme> LoadThemesFromAssembly(List<Theme> themes, Assembly assembly)
        {
            Theme theme;
            List<Type> themeTypes = new List<Type>();

            Type[] themeControlTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IThemeControl))).ToArray();
            foreach (Type themeControlType in themeControlTypes)
            {
                // Check if type should be ignored
                if (themeControlType.IsOqtaneIgnore() || 
                    themeControlType.GetInterfaces().Contains(typeof(ILayoutControl)) || // deprecated 
                    themeControlType.GetInterfaces().Contains(typeof(IContainerControl))) continue;

                // create namespace root typename
                string qualifiedThemeType = themeControlType.Namespace + ", " + themeControlType.Assembly.GetName().Name;

                int index = themes.FindIndex(item => item.ThemeName == qualifiedThemeType);
                if (index == -1)
                {
                    // Find all types in the assembly with the same namespace root
                    themeTypes = assembly.GetTypes()
                        .Where(item => !item.IsOqtaneIgnore())
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace == themeControlType.Namespace || item.Namespace.StartsWith(themeControlType.Namespace + "."))
                        .ToList();

                    // determine if this theme implements ITheme
                    Type themetype = themeTypes
                        .FirstOrDefault(item => item.GetInterfaces().Contains(typeof(ITheme)));
                    if (themetype != null)
                    {
                        var themeobject = Activator.CreateInstance(themetype) as ITheme;
                        theme = themeobject.Theme;
                    }
                    else
                    {
                        theme = new Theme
                        {
                            Name = themeControlType.Name,
                            Version = new Version(1, 0, 0).ToString()
                        };
                    }

                    // set internal properties
                    theme.ThemeName = qualifiedThemeType;
                    theme.Themes = new List<ThemeControl>();
                    theme.Containers = new List<ThemeControl>();
                    theme.AssemblyName = assembly.FullName.Split(",")[0];
                    if (theme.Resources != null)
                    {
                        foreach (var resource in theme.Resources)
                        {
                            if (resource.Url.StartsWith("~"))
                            {
                                resource.Url = resource.Url.Replace("~", "/Themes/" + Utilities.GetTypeName(theme.ThemeName) + "/").Replace("//", "/");
                            }
                        }
                    }
                    Debug.WriteLine($"Oqtane Info: Registering Theme {theme.ThemeName}");
                    themes.Add(theme);
                    index = themes.FindIndex(item => item.ThemeName == qualifiedThemeType);
                }
                theme = themes[index];

                var themecontrolobject = Activator.CreateInstance(themeControlType) as IThemeControl;
                theme.Themes.Add(
                    new ThemeControl
                    {
                        TypeName = themeControlType.FullName + ", " + themeControlType.Assembly.GetName().Name,
                        Name = ((string.IsNullOrEmpty(themecontrolobject.Name)) ? Utilities.GetTypeNameLastSegment(themeControlType.FullName, 0) : themecontrolobject.Name),
                        Thumbnail = themecontrolobject.Thumbnail,
                        Panes = themecontrolobject.Panes
                    }
                );

                // containers
                Type[] containertypes = themeTypes
                    .Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                foreach (Type containertype in containertypes)
                {
                    var containerobject = Activator.CreateInstance(containertype) as IThemeControl;
                    if (theme.Containers.FirstOrDefault(item => item.TypeName == containertype.FullName + ", " + themeControlType.Assembly.GetName().Name) == null)
                    {
                        theme.Containers.Add(
                            new ThemeControl
                            {
                                TypeName = containertype.FullName + ", " + themeControlType.Assembly.GetName().Name,
                                Name = (string.IsNullOrEmpty(containerobject.Name)) ? Utilities.GetTypeNameLastSegment(containertype.FullName, 0) : containerobject.Name,
                                Thumbnail = containerobject.Thumbnail,
                                Panes = ""
                            }
                        );
                    }
                }

                themes[index] = theme;
            }
            return themes;
        }
    }
}
