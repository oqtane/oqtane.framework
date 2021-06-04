using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Module _Instance_ which will be shown on a page. This is different from a <see cref="ModuleDefinition"/> which describes a Module.
    /// </summary>
    public class Module : IAuditable
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

        [NotMapped]
        public string DeletedBy { get; set; }
        [NotMapped]
        public DateTime? DeletedOn { get; set; }
        [NotMapped]
        public bool IsDeleted { get; set; }
        
        #endregion
        
        [NotMapped]
        public string Permissions { get; set; }

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
        // TODO: unclear why these are IModuleControl properties - there is no such interface
        [NotMapped]
        public SecurityAccessLevel SecurityAccessLevel { get; set; }
        [NotMapped]
        public string ControlTitle { get; set; }
        [NotMapped]
        public string Actions { get; set; }
        [NotMapped]
        public bool UseAdminContainer { get; set; }

        #endregion
    }
}
