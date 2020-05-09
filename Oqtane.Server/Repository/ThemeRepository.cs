using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Themes;

namespace Oqtane.Repository
{
    public class ThemeRepository : IThemeRepository
    {
        private List<Theme> _themes; // lazy load

        public IEnumerable<Theme> GetThemes()
        {
            return LoadThemes();
        }

        private List<Theme> LoadThemes()
        {
            if (_themes == null)
            {
                // get themes
                _themes = LoadThemesFromAssemblies();
            }
            return _themes;
        }

        private List<Theme> LoadThemesFromAssemblies()
        {
            List<Theme> themes = new List<Theme>();

            // iterate through Oqtane theme assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
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
                // Check if type should be ignored
                if (themeControlType.Name == "ThemeBase"
                    || themeControlType.IsGenericType
                    || Attribute.IsDefined(themeControlType, typeof(OqtaneIgnoreAttribute))
                ) continue;

                string themeNamespace = themeControlType.Namespace;
                string qualifiedModuleType = themeNamespace + ", " + themeControlType.Assembly.GetName().Name;

                int index = themes.FindIndex(item => item.ThemeName == themeNamespace);
                if (index == -1)
                {
                    // determine if this theme implements ITheme
                    Type themetype = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(themeNamespace))
                        .Where(item => item.GetInterfaces().Contains(typeof(ITheme))).FirstOrDefault();
                    if (themetype != null)
                    {
                        var themeobject = Activator.CreateInstance(themetype);
                        theme = (Theme)themetype.GetProperty("Theme").GetValue(themeobject);
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
                    theme.ThemeName = themeNamespace;
                    theme.ThemeControls = "";
                    theme.PaneLayouts = "";
                    theme.ContainerControls = "";
                    theme.AssemblyName = assembly.FullName.Split(",")[0];
                    themes.Add(theme);
                    index = themes.FindIndex(item => item.ThemeName == themeNamespace);
                }
                theme = themes[index];
                theme.ThemeControls += (themeControlType.FullName + ", " + themeControlType.Assembly.GetName().Name + ";");

                // layouts
                Type[] layouttypes = assembly.GetTypes()
                    .Where(item => item.Namespace != null)
                    .Where(item => item.Namespace.StartsWith(themeNamespace))
                    .Where(item => item.GetInterfaces().Contains(typeof(ILayoutControl))).ToArray();
                foreach (Type layouttype in layouttypes)
                {
                    string panelayout = layouttype.FullName + ", " + themeControlType.Assembly.GetName().Name + ";";
                    if (!theme.PaneLayouts.Contains(panelayout))
                    {
                        theme.PaneLayouts += panelayout;
                    }
                }

                // containers
                Type[] containertypes = assembly.GetTypes()
                    .Where(item => item.Namespace != null)
                    .Where(item => item.Namespace.StartsWith(themeNamespace))
                    .Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                foreach (Type containertype in containertypes)
                {
                    string container = containertype.FullName + ", " + themeControlType.Assembly.GetName().Name + ";";
                    if (!theme.ContainerControls.Contains(container))
                    {
                        theme.ContainerControls += container;
                    }
                }

                themes[index] = theme;
            }
            return themes;
        }
    }
}
