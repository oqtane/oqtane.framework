using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Site in a <see cref="Tenant"/> in an Oqtane installation.
    /// Sites can have multiple <see cref="Alias"/>es.
    /// </summary>
    public class Site : IAuditable, IDeletable
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
        /// Determines if users may register / create accounts
        /// </summary>
        public bool AllowRegistration { get; set; }

        /// <summary>
        /// Unique GUID to identify the Site.
        /// </summary>
        public string SiteGuid { get; set; }

        #region IAuditable Properties

        /// <inheritdoc/>
        public string CreatedBy { get; set; }
        /// <inheritdoc/>
        public DateTime CreatedOn { get; set; }
        /// <inheritdoc/>
        public string ModifiedBy { get; set; }
        /// <inheritdoc/>
        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Extended IAuditable Properties, may be moved to an Interface some day so not documented yet

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
        
        #endregion
        
        [NotMapped]
        public string SiteTemplateType { get; set; }

        [NotMapped]
        [Obsolete("This property is deprecated.", false)]
        public string DefaultLayoutType { get; set; }
    }
}
