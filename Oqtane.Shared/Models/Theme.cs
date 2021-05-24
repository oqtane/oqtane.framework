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
            Template = "";
            ThemeSettingsType = "";
            ContainerSettingsType = "";
            PackageName = "";
        }

        public string ThemeName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Url { get; set; }
        public string Contact { get; set; }
        public string License { get; set; }
        public string Dependencies { get; set; }
        public string ThemeSettingsType { get; set; } // added in 2.0.2
        public string ContainerSettingsType { get; set; } // added in 2.0.2
        public string PackageName { get; set; } // added in 2.1.0

        // internal properties
        public string AssemblyName { get; set; }
        public List<ThemeControl> Themes { get; set; }
        public List<ThemeControl> Containers { get; set; }
        public string Template { get; set; }

        #region Obsolete Properties

        [Obsolete("This property is obsolete. Use Themes instead.", false)]
        public string ThemeControls { get; set; }
        [Obsolete("This property is obsolete. Use Layouts instead.", false)]
        public string PaneLayouts { get; set; }
        [Obsolete("This property is obsolete. Use Containers instead.", false)]
        public string ContainerControls { get; set; }
        [Obsolete("This property is obsolete.", false)]
        public List<ThemeControl> Layouts { get; set; }

        #endregion

    }
}
