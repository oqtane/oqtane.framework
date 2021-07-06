using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Page in Oqtane
    /// </summary>
    public class Page : IAuditable, IDeletable
    {
        /// <summary>
        /// Id of the Page
        /// </summary>
        public int PageId { get; set; }
        
        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the parent <see cref="Page"/> if it has one.
        /// </summary>
        public int? ParentId { get; set; }
        
        /// <summary>
        /// Page Name.
        /// TODO: todoc where this is used
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Page Title which is shown in the browser tab.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Path of the page.
        /// TODO: todoc relative to what? site root, parent-page, domain?
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// Sort order in the list of other sibling pages
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Full URL to this page.
        /// TODO: verify that this is the case - does it contain domain etc. or just from domain or alias root?
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Reference to a <see cref="Theme"/> which will be used to show this page.
        /// </summary>
        public string ThemeType { get; set; }
        
        /// <summary>
        /// Reference to a Container which will be used for modules on this page.
        /// </summary>
        public string DefaultContainerType { get; set; }
        
        /// <summary>
        /// Icon file for this page.
        /// TODO: unclear what this is for, and what icon library is used. Probably FontAwesome?
        /// </summary>
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsClickable { get; set; }
        public int? UserId { get; set; }
        public bool IsPersonalizable { get; set; }

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

        /// <summary>
        /// List of Pane-names which this Page has.
        /// </summary>
        [NotMapped]
        public List<string> Panes { get; set; }

        /// <summary>
        /// List of <see cref="Resource"/> (CSS, JS) which this page needs to render properly. 
        /// </summary>
        [NotMapped]
        public List<Resource> Resources { get; set; }
        [NotMapped]
        public string Permissions { get; set; }
        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }
        [NotMapped]
        public int Level { get; set; }

        /// <summary>
        /// Determines if there are sub-pages. True if this page has sub-pages.
        /// </summary>
        [NotMapped]
        public bool HasChildren { get; set; }

        #region Deprecated Properties

        [Obsolete("This property is deprecated", false)]
        [NotMapped]
        public bool EditMode { get; set; }

        [Obsolete("This property is deprecated", false)]
        [NotMapped]
        public string LayoutType { get; set; }

        #endregion
    }
}
