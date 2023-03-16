using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Page in Oqtane
    /// </summary>
    public class Page : ModelBase, IDeletable
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
        /// Meta tags to be included in the head of the page
        /// </summary>
        public string Meta { get; set; }

        /// <summary>
        /// Icon file for this page.
        /// TODO: unclear what this is for, and what icon library is used. Probably FontAwesome?
        /// </summary>
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsClickable { get; set; }
        public int? UserId { get; set; }
        public bool IsPersonalizable { get; set; }

        #region IDeletable Properties

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
        public List<Permission> PermissionList { get; set; }

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

        [Obsolete("The EditMode property is deprecated", false)]
        [NotMapped]
        public bool EditMode { get; set; }

        [Obsolete("The LayoutType property is deprecated", false)]
        [NotMapped]
        public string LayoutType { get; set; }

        [Obsolete("The Permissions property is deprecated. Use PermissionList instead", false)]
        [NotMapped]
        [JsonIgnore] // exclude from API payload
        public string Permissions {
            get
            {
                return JsonSerializer.Serialize(PermissionList);
            }
            set
            {
                PermissionList = JsonSerializer.Deserialize<List<Permission>>(value);
            }
        }

        #endregion
    }
}
