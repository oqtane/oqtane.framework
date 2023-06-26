using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Information about a Theme in Oqtane.
    /// </summary>
    public class Theme : ModelBase
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
            Resources = null;
        }

        /// <summary>
        /// Reference to the <see cref="Theme"/>.
        /// </summary>
        public int ThemeId { get; set; }

        /// <summary>
        /// Full Namespace / Identifier of the Theme.
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        /// Friendly name to show in UI
        /// </summary>
        public string Name { get; set; }

        // additional ITheme properties 
        [NotMapped]
        public string Version { get; set; }

        [NotMapped]
        public string Owner { get; set; }

        [NotMapped]
        public string Url { get; set; }

        [NotMapped]
        public string Contact { get; set; }

        [NotMapped]
        public string License { get; set; }

        [NotMapped]
        public string Dependencies { get; set; }

        [NotMapped]
        public string ThemeSettingsType { get; set; } // added in 2.0.2

        [NotMapped]
        public string ContainerSettingsType { get; set; } // added in 2.0.2

        [NotMapped]
        public string PackageName { get; set; } // added in 2.1.0

        [NotMapped]
        public List<Resource> Resources { get; set; } // added in 4.0.0

        [NotMapped]
        public bool IsAutoEnabled { get; set; } = true; // added in 4.0.0


        // internal properties
        [NotMapped]
        public int SiteId { get; set; }
        [NotMapped]
        public bool IsEnabled { get; set; }
        [NotMapped]
        public string AssemblyName { get; set; }
        [NotMapped]
        public List<ThemeControl> Themes { get; set; }
        [NotMapped]
        public List<ThemeControl> Containers { get; set; }
        [NotMapped]
        public string Template { get; set; }

        #region Obsolete Properties

        [Obsolete("This property is obsolete. Use Themes instead.", false)]
        [NotMapped]
        public string ThemeControls { get; set; }
        [Obsolete("This property is obsolete. Use Layouts instead.", false)]
        [NotMapped]
        public string PaneLayouts { get; set; }
        [Obsolete("This property is obsolete. Use Containers instead.", false)]
        [NotMapped]
        public string ContainerControls { get; set; }
        [Obsolete("This property is obsolete.", false)]
        [NotMapped]
        public List<ThemeControl> Layouts { get; set; }

        #endregion

    }
}
