using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Themes;

namespace Oqtane.Repository
{
    public interface IThemeRepository
    {
        IEnumerable<Theme> GetThemes(int siteId);
        Theme GetTheme(int themeId, int siteId);
        void UpdateTheme(Theme theme);
        void DeleteTheme(int themeId);
        List<Theme> FilterThemes(List<Theme> themes);
    }

    public class ThemeRepository : IThemeRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;
        private readonly IPermissionRepository _permissions;
        private readonly ITenantManager _tenants;
        private readonly ISettingRepository _settings;
        private readonly IServerStateManager _serverState;
        private readonly string settingprefix = "SiteEnabled:";

        public ThemeRepository(MasterDBContext context, IMemoryCache cache, IPermissionRepository permissions, ITenantManager tenants, ISettingRepository settings, IServerStateManager serverState)
        {
            _db = context;
            _cache = cache;
            _permissions = permissions;
            _tenants = tenants;
            _settings = settings;
            _serverState = serverState;
        }

        public IEnumerable<Theme> GetThemes(int siteId)
        {
            return LoadThemes(siteId);
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
            _permissions.UpdatePermissions(theme.SiteId, EntityNames.Theme, theme.ThemeId, theme.PermissionList);

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
                Theme.PermissionList = theme.PermissionList;
                Theme.Fingerprint = Utilities.GenerateSimpleHash(theme.ModifiedOn.ToString("yyyyMMddHHmm"));
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
                    if (theme.Version != Theme.Version)
                    {
                        // update theme version
                        theme.Version = Theme.Version;
                        _db.SaveChanges();
                    }

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
                var assemblies = new List<string>();

                // get all module definition permissions for site
                List<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.Theme).ToList();

                // get settings for site
                var settings = _settings.GetSettings(EntityNames.Theme).ToList();

                // populate theme site settings
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
                        if (!assemblies.Contains(theme.AssemblyName))
                        {
                            assemblies.Add(theme.AssemblyName);
                        }
                        if (!string.IsNullOrEmpty(theme.Dependencies))
                        {
                            foreach (var assembly in theme.Dependencies.Replace(".dll", "").Split(',', StringSplitOptions.RemoveEmptyEntries).Reverse())
                            {
                                if (!assemblies.Contains(assembly.Trim()))
                                {
                                    assemblies.Insert(0, assembly.Trim());
                                }
                            }
                        }
                    }

                    if (permissions.Count == 0)
                    {
                        // no module definition permissions exist for this site
                        theme.PermissionList = ClonePermissions(siteId, theme.PermissionList);
                        _permissions.UpdatePermissions(siteId, EntityNames.Theme, theme.ThemeId, theme.PermissionList);
                    }
                    else
                    {
                        if (permissions.Any(item => item.EntityId == theme.ThemeId))
                        {
                            theme.PermissionList = permissions.Where(item => item.EntityId == theme.ThemeId).ToList();
                        }
                        else
                        {
                            // permissions for theme do not exist for this site
                            theme.PermissionList = ClonePermissions(siteId, theme.PermissionList);
                            _permissions.UpdatePermissions(siteId, EntityNames.Theme, theme.ThemeId, theme.PermissionList);
                        }
                    }
                }

                // cache site assemblies
                var serverState = _serverState.GetServerState(siteKey);
                foreach (var assembly in assemblies)
                {
                    if (!serverState.Assemblies.Contains(assembly)) serverState.Assemblies.Add(assembly);
                }

                // clean up any orphaned permissions
                var ids = new HashSet<int>(Themes.Select(item => item.ThemeId));
                foreach (var permission in permissions.Where(item => !ids.Contains(item.EntityId)))
                {
                    try
                    {
                        _permissions.DeletePermission(permission.PermissionId);
                    }
                    catch
                    {
                        // multi-threading can cause a race condition to occur
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

            Type[] themeTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ITheme))).ToArray();
            Type[] themeControlTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IThemeControl))).ToArray();
            Type[] containerControlTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();

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
                    // determine if this component is part of a theme which implements ITheme
                    Type themetype = themeTypes.FirstOrDefault(item => item.Namespace == themeControlType.Namespace);

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

                    // default permissions
                    theme.PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.Utilize, RoleNames.Admin, true),
                        new Permission(PermissionNames.Utilize, RoleNames.Registered, true)
                    };

                    Debug.WriteLine($"Oqtane Info: Registering Theme {theme.ThemeName}");
                    themes.Add(theme);
                    index = themes.FindIndex(item => item.ThemeName == qualifiedThemeType);
                }
                theme = themes[index];

                // add theme control
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

                if (!theme.Containers.Any())
                {
                    // add container controls
                    foreach (Type containertype in containerControlTypes.Where(item => item.Namespace == themeControlType.Namespace))
                    {
                        var containerobject = Activator.CreateInstance(containertype) as IThemeControl;
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

        private List<Permission> ClonePermissions(int siteId, List<Permission> permissionList)
        {
            var permissions = new List<Permission>();
            if (permissionList != null)
            {
                foreach (var p in permissionList)
                {
                    var permission = new Permission();
                    permission.SiteId = siteId;
                    permission.EntityName = p.EntityName;
                    permission.EntityId = p.EntityId;
                    permission.PermissionName = p.PermissionName;
                    permission.RoleId = null;
                    permission.RoleName = p.RoleName;
                    permission.UserId = p.UserId;
                    permission.IsAuthorized = p.IsAuthorized;
                    permissions.Add(permission);
                }
            }
            return permissions;
        }
    }
}
