using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Module _Instance_ which will be shown on a page. This is different from a <see cref="ModuleDefinition"/> which describes a Module.
    /// </summary>
    public class Module : ModelBase
    {
        /// <summary>
        /// The ID of this instance
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the <see cref="ModuleDefinition"/>
        /// </summary>
        public string ModuleDefinitionName { get; set; }

        /// <summary>
        /// Determines if this Module Instance should be shown on all pages of the current <see cref="Site"/>
        /// </summary>
        public bool AllPages { get; set; }

        #region IDeletable Properties (note that these are NotMapped and are only used for storing PageModule properties)

        [NotMapped]
        public string DeletedBy { get; set; }
        [NotMapped]
        public DateTime? DeletedOn { get; set; }
        [NotMapped]
        public bool IsDeleted { get; set; }
        
        #endregion
        
        [NotMapped]
        public List<Permission> PermissionList { get; set; }

        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }

        #region PageModule properties

        [NotMapped]
        public int PageModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Page"/> this module is on.
        /// </summary>
        [NotMapped]
        public int PageId { get; set; }

        [NotMapped]
        public string Title { get; set; }

        /// <summary>
        /// The Pane this module is shown in.
        /// </summary>
        [NotMapped]
        public string Pane { get; set; }

        [NotMapped]
        public int Order { get; set; }

        [NotMapped]
        public string ContainerType { get; set; }

        #endregion

        #region SiteRouter properties
        
        [NotMapped]
        public string ModuleType { get; set; }
        [NotMapped]
        public int PaneModuleIndex { get; set; }
        [NotMapped]
        public int PaneModuleCount { get; set; }

        #endregion

        #region ModuleDefinition
        /// <summary>
        /// Reference to the <see cref="ModuleDefinition"/> used for this module.
        /// TODO: todoc - unclear if this is always populated
        /// </summary>
        [NotMapped]
        public ModuleDefinition ModuleDefinition { get; set; }

        #endregion

        #region IModuleControl properties
        [NotMapped]
        public SecurityAccessLevel SecurityAccessLevel { get; set; }
        [NotMapped]
        public string ControlTitle { get; set; }
        [NotMapped]
        public string Actions { get; set; }
        [NotMapped]
        public bool UseAdminContainer { get; set; }

        #endregion

        #region Deprecated Properties

        [Obsolete("The Permissions property is deprecated. Use PermissionList instead", false)]
        [NotMapped]
        [JsonIgnore] // exclude from API payload
        public string Permissions
        {
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
