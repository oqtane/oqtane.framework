using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Reflection;
using System;
using Oqtane.Themes;

namespace Oqtane.Repository
{
    public class ThemeRepository : IThemeRepository
    {
        private readonly List<Theme> themes;

        public ThemeRepository()
        {
            themes = LoadThemes();
        }

        private List<Theme> LoadThemes()
        {
            List<Theme> themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Theme.")).ToArray();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                themes = LoadThemesFromAssembly(themes, assembly);
            }

            return themes;
        }

        private List<Theme> LoadThemesFromAssembly(List<Theme> themes, Assembly assembly)
        {
            Theme theme;
            Type[] themeControlTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IThemeControl))).ToArray();
            foreach (Type themeControlType in themeControlTypes)
            {
                if (themeControlType.Name != "ThemeBase")
                {
                    string[] typename = themeControlType.AssemblyQualifiedName.Split(',').Select(item => item.Trim()).ToList().ToArray();
                    string[] segments = typename[0].Split('.');
                    Array.Resize(ref segments, segments.Length - 1);
                    string Namespace = string.Join(".", segments);

                    int index = themes.FindIndex(item => item.ThemeName == Namespace);
                    if (index == -1)
                    {
                        /// determine if this theme implements ITheme
                        Type themeType = assembly.GetTypes()
                            .Where(item => item.Namespace != null)
                            .Where(item => item.Namespace.StartsWith(Namespace))
                            .Where(item => item.GetInterfaces().Contains(typeof(ITheme))).FirstOrDefault();
                        if (themeType != null)
                        {
                            var themeObject = Activator.CreateInstance(themeType);
                            theme = new Theme
                            {
                                ThemeName = Namespace,
                                Name = (string)themeType.GetProperty("Name").GetValue(themeObject),
                                Version = (string)themeType.GetProperty("Version").GetValue(themeObject),
                                Owner = (string)themeType.GetProperty("Owner").GetValue(themeObject),
                                Url = (string)themeType.GetProperty("Url").GetValue(themeObject),
                                Contact = (string)themeType.GetProperty("Contact").GetValue(themeObject),
                                License = (string)themeType.GetProperty("License").GetValue(themeObject),
                                Dependencies = (string)themeType.GetProperty("Dependencies").GetValue(themeObject),
                                ThemeControls = "",
                                PaneLayouts = "",
                                ContainerControls = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        else
                        {
                            theme = new Theme
                            {
                                ThemeName = Namespace,
                                Name = themeControlType.Name,
                                Version = new Version(1, 0, 0).ToString(),
                                Owner = "",
                                Url = "",
                                Contact = "",
                                License = "",
                                Dependencies = "",
                                ThemeControls = "",
                                PaneLayouts = "",
                                ContainerControls = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        themes.Add(theme);
                        index = themes.FindIndex(item => item.ThemeName == Namespace);
                    }
                    theme = themes[index];
                    // layouts and themes
                    if (themeControlType.FullName.EndsWith("Layout"))
                    {
                        theme.PaneLayouts += (themeControlType.FullName + ", " + typename[1] + ";");
                    }
                    else
                    {
                        theme.ThemeControls += (themeControlType.FullName + ", " + typename[1] + ";");
                    }
                    // containers
                    Type[] containertypes = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(Namespace))
                        .Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                    foreach (Type containertype in containertypes)
                    {
                        theme.ContainerControls += (containertype.FullName + ", " + typename[1] + ";");
                    }
                    themes[index] = theme;
                }
            }
            return themes;
        }

        public IEnumerable<Theme> GetThemes()
        {
            return themes;
        }
    }
}
