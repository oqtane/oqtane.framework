using System;
using System.Collections.Generic;

namespace Oqtane.Models
{
    public class Theme
    {
        public Theme()
        {
            Name = "";
            Version = "";
            Owner = "";
            Url = "";
            Contact = "";
            License = "";
            Dependencies = "";
        }

        public string ThemeName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Url { get; set; }
        public string Contact { get; set; }
        public string License { get; set; }
        public string Dependencies { get; set; }
        public string AssemblyName { get; set; }
        public List<ThemeControl> Themes { get; set; }
        public List<ThemeControl> Layouts { get; set; }
        public List<ThemeControl> Containers { get; set; }

        //[Obsolete("This property is obsolete. Use Themes instead.", false)]
        public string ThemeControls { get; set; }
        //[Obsolete("This property is obsolete. Use Layouts instead.", false)]
        public string PaneLayouts { get; set; }
        //[Obsolete("This property is obsolete. Use Containers instead.", false)]
        public string ContainerControls { get; set; }
    }
}
