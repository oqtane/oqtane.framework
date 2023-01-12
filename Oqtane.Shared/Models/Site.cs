using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Site in a <see cref="Tenant"/> in an Oqtane installation.
    /// Sites can have multiple <see cref="Alias"/>es.
    /// </summary>
    public class Site : ModelBase, IDeletable
    {
        /// <summary>
        /// Internal ID, not to be confused with the <see cref="Alias.AliasId"/>
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Tenant"/> the Site is in
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The site Name
        /// TODO: todoc where this will be used / shown
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reference to a <see cref="File"/> which has the Logo for this site.
        /// Should be an image.
        /// The theme can then use this where needed. 
        /// </summary>
        public int? LogoFileId { get; set; }

        /// <summary>
        /// Reference to a <see cref="File"/> which has the FavIcon for this site.
        /// Should be an image. 
        /// The theme can then use this where needed.
        /// TODO: todoc does this get applied automatically, or does the Theme do this?
        /// </summary>
        public int? FaviconFileId { get; set; }

        public string DefaultThemeType { get; set; }
        public string DefaultContainerType { get; set; }
        public string AdminContainerType { get; set; }
        public bool PwaIsEnabled { get; set; }
        public int? PwaAppIconFileId { get; set; }
        public int? PwaSplashIconFileId { get; set; }

        /// <summary>
        /// Determines if visitors may register / create user accounts
        /// </summary>
        public bool AllowRegistration { get; set; }

        /// <summary>
        /// Determines if visitors will be tracked
        /// </summary>
        public bool VisitorTracking { get; set; }

        /// <summary>
        /// Determines if broken urls (404s) will be captured automatically
        /// </summary>
        public bool CaptureBrokenUrls { get; set; }

        /// <summary>
        /// Unique GUID to identify the Site.
        /// </summary>
        public string SiteGuid { get; set; }

        /// <summary>
        /// The hosting model for the site (ie. Server or WebAssembly ).
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// The render mode for the site (ie. Server, ServerPrerendered, WebAssembly, WebAssemblyPrerendered ).
        /// </summary>
        public string RenderMode { get; set; }

        /// <summary>
        /// Keeps track of site configuration changes and is used by the IUpgradeable interface
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The home page of the site which will be used as a fallback if no page has a path of "/" 
        /// </summary>
        public int? HomePageId { get; set; }

        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }

        [NotMapped]
        public List<Page> Pages { get; set; }

        [NotMapped]
        public List<Module> Modules { get; set; }

        [NotMapped]
        public List<Language> Languages { get; set; }

        #region IDeletable Properties

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        [NotMapped]
        public string SiteTemplateType { get; set; }

        #region Obsolete properties
        [NotMapped]
        [Obsolete("This property is deprecated.", false)]
        public string DefaultLayoutType { get; set; }
        #endregion
    }
}
