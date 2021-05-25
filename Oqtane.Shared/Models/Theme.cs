using System;
using System.Collections.Generic;

namespace Oqtane.Models
{
    /// <summary>
    /// Information about a Theme in Oqtane.
    /// </summary>
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

        /// <summary>
        /// Full Namespace / Identifier of the Theme.
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        /// Nice Name of the Theme.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version as determined by the DLL / NuGet Package.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Author / Creator of the Theme. 
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// URL (in NuGet) of the Theme
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Author Contact information
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// Theme License, like `MIT` etc.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Theme Dependencies (DLLs) which the system will check if they exist
        /// </summary>
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
