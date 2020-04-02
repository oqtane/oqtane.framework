using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oqtane.Models;
using Oqtane.Themes;

namespace Oqtane.Repository
{
    public class ThemeRepository : IThemeRepository
    {
        private List<Theme> LoadThemes()
        {
            List<Theme> themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Theme.")).ToArray();
            foreach (Assembly assembly in assemblies)
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
                    string @namespace = string.Join(".", segments);

                    int index = themes.FindIndex(item => item.ThemeName == @namespace);
                    if (index == -1)
                    {
                        // determine if this theme implements ITheme
                        Type themetype = assembly.GetTypes()
                            .Where(item => item.Namespace != null)
                            .Where(item => item.Namespace.StartsWith(@namespace))
                            .Where(item => item.GetInterfaces().Contains(typeof(ITheme))).FirstOrDefault();
                        if (themetype != null)
                        {
                            var themeobject = Activator.CreateInstance(themetype);
                            var properties = (Dictionary<string, string>)themetype.GetProperty("Properties").GetValue(themeobject);
                            var moduleDefinition = (ModuleDefinition)themetype.GetProperty("ModuleDefinition").GetValue(themeobject);
                            if (properties == null || properties.Count == 0)
                            {
                                theme = new Theme
                                {
                                    ThemeName = @namespace,
                                    Name = moduleDefinition.Name,
                                    Version = moduleDefinition.Version,
                                    Owner = moduleDefinition.Owner,
                                    Url = moduleDefinition.Url,
                                    Contact = moduleDefinition.Contact,
                                    License = moduleDefinition.License,
                                    Dependencies = moduleDefinition.Dependencies,
                                    ThemeControls = string.Empty,
                                    PaneLayouts = string.Empty,
                                    ContainerControls = string.Empty,
                                    AssemblyName = moduleDefinition.AssemblyName
                                };
                            }
                            else
                            {
                                theme = new Theme
                                {
                                    ThemeName = @namespace,
                                    Name = GetProperty(properties, "Name"),
                                    Version = GetProperty(properties, "Version"),
                                    Owner = GetProperty(properties, "Owner"),
                                    Url = GetProperty(properties, "Url"),
                                    Contact = GetProperty(properties, "Contact"),
                                    License = GetProperty(properties, "License"),
                                    Dependencies = GetProperty(properties, "Dependencies"),
                                    ThemeControls = string.Empty,
                                    PaneLayouts = string.Empty,
                                    ContainerControls = string.Empty,
                                    AssemblyName = assembly.FullName.Split(",")[0]
                                };
                            }
                        }
                        else
                        {
                            theme = new Theme
                            {
                                ThemeName = @namespace,
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
                        index = themes.FindIndex(item => item.ThemeName == @namespace);
                    }
                    theme = themes[index];
                    theme.ThemeControls += (themeControlType.FullName + ", " + typename[1] + ";");

                    // layouts
                    Type[] layouttypes = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(@namespace))
                        .Where(item => item.GetInterfaces().Contains(typeof(ILayoutControl))).ToArray();
                    foreach (Type layouttype in layouttypes)
                    {
                        string panelayout = layouttype.FullName + ", " + typename[1] + ";";
                        if (!theme.PaneLayouts.Contains(panelayout))
                        {
                            theme.PaneLayouts += panelayout;
                        }
                    }

                    // containers
                    Type[] containertypes = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(@namespace))
                        .Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                    foreach (Type containertype in containertypes)
                    {
                        string container = containertype.FullName + ", " + typename[1] + ";";
                        if (!theme.ContainerControls.Contains(container))
                        {
                            theme.ContainerControls += container;
                        }
                    }

                    themes[index] = theme;
                }
            }
            return themes;
        }

        private string GetProperty(Dictionary<string, string> properties, string key)
        {
            string value = "";
            if (properties.ContainsKey(key))
            {
                value = properties[key];
            }
            return value;
        }

        public IEnumerable<Theme> GetThemes()
        {
            return LoadThemes();
        }
    }
}
