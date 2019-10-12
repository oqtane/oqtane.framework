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
        private List<Theme> LoadThemes()
        {
            List<Theme> Themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Theme.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                Themes = LoadThemesFromAssembly(Themes, assembly);
            }

            return Themes;
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
                        Type themetype = assembly.GetTypes()
                            .Where(item => item.Namespace != null)
                            .Where(item => item.Namespace.StartsWith(Namespace))
                            .Where(item => item.GetInterfaces().Contains(typeof(ITheme))).FirstOrDefault();
                        if (themetype != null)
                        {
                            var themeobject = Activator.CreateInstance(themetype);
                            Dictionary<string, string> properties = (Dictionary<string, string>)themetype.GetProperty("Properties").GetValue(themeobject);
                            theme = new Theme
                            {
                                ThemeName = Namespace,
                                Name = GetProperty(properties, "Name"),
                                Version = GetProperty(properties, "Version"),
                                Owner = GetProperty(properties, "Owner"),
                                Url = GetProperty(properties, "Url"),
                                Contact = GetProperty(properties, "Contact"),
                                License = GetProperty(properties, "License"),
                                Dependencies = GetProperty(properties, "Dependencies"),
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
                    theme.ThemeControls += (themeControlType.FullName + ", " + typename[1] + ";");

                    // layouts
                    Type[] layouttypes = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(Namespace))
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
                        .Where(item => item.Namespace.StartsWith(Namespace))
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

        private string GetProperty(Dictionary<string, string> Properties, string Key)
        {
            string Value = "";
            if (Properties.ContainsKey(Key))
            {
                Value = Properties[Key];
            }
            return Value;
        }

        public IEnumerable<Theme> GetThemes()
        {
            return LoadThemes();
        }
    }
}
