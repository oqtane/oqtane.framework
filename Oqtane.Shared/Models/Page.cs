using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        /// Path of the page.
        /// TODO: todoc relative to what? site root, parent-page, domain?
        /// </summary>
        public string Path { get; set; }

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
        /// Content to be included in the head of the page
        /// </summary>
        public string HeadContent { get; set; }

        /// <summary>
        /// Content to be included in the body of the page
        /// </summary>
        public string BodyContent { get; set; }

        /// <summary>
        /// Icon class name for this page
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Indicates if this page should be included in navigation menu
        /// </summary>
        public bool IsNavigation { get; set; }

        /// <summary>
        /// Indicates if this page should be clickable in navigation menu
        /// </summary>
        public bool IsClickable { get; set; }

        /// <summary>
        /// Indicates if page is personalizable ie. allows users to create custom versions of the page
        /// </summary>
        public bool IsPersonalizable { get; set; }

        /// <summary>
        /// Reference to the user <see cref="User"/> who owns the personalized page
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Start of when this page is visible. See also <see cref="ExpiryDate"/>
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// End of when this page is visible. See also <see cref="EffectiveDate"/>
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// The hierarchical level of the page
        /// </summary>
        [NotMapped]
        public int Level { get; set; }

        /// <summary>
        /// Determines if there are sub-pages. True if this page has sub-pages.
        /// </summary>
        [NotMapped]
        public bool HasChildren { get; set; }

        /// <summary>
        /// Indicates if module permissions should be updated to be consistent with page permissions
        /// </summary>
        [NotMapped]
        public bool UpdateModulePermissions { get; set; }

        /// <summary>
        /// List of permissions for this page
        /// </summary>
        [NotMapped]
        public List<Permission> PermissionList { get; set; }

        /// <summary>
        /// List of settings for this page
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }

        #region SiteRouter properties

        /// <summary>
        /// List of Pane names for the Theme assigned to this page
        /// </summary>
        [NotMapped]
        public List<string> Panes { get; set; }

        /// <summary>
        /// List of <see cref="Resource"/> (CSS, JS) which this page needs to render properly. 
        /// </summary>
        [NotMapped]
        public List<Resource> Resources { get; set; }

        #endregion

        #region IDeletable Properties

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

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

        public Page Clone()
        {
            return new Page
            {
                PageId = PageId,
                SiteId = SiteId,
                Path = Path,
                ParentId = ParentId,
                Name = Name,
                Title = Title,
                Order = Order,
                Url = Url,
                ThemeType = ThemeType,
                DefaultContainerType = DefaultContainerType,
                HeadContent = HeadContent,
                BodyContent = BodyContent,
                Icon = Icon, 
                IsNavigation = IsNavigation,
                IsClickable = IsClickable,
                UserId = UserId,
                IsPersonalizable = IsPersonalizable,
                EffectiveDate = EffectiveDate,
                ExpiryDate = ExpiryDate,
                Level = Level,
                HasChildren = HasChildren,                 
                CreatedBy = CreatedBy,
                CreatedOn = CreatedOn,
                ModifiedBy = ModifiedBy,
                ModifiedOn = ModifiedOn,
                DeletedBy = DeletedBy,
                DeletedOn = DeletedOn,
                IsDeleted = IsDeleted,
                PermissionList = PermissionList.ConvertAll(permission => permission.Clone()),
                Settings = Settings.ToDictionary(setting => setting.Key, setting => setting.Value)
            };
        }
    }
}
