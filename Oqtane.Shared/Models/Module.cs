using Oqtane.Shared;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Module : IAuditable
    {
        public int ModuleId { get; set; }
        public int SiteId { get; set; }
        public string ModuleDefinitionName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        
        [NotMapped]
        public string DeletedBy { get; set; }
        [NotMapped]
        public DateTime? DeletedOn { get; set; }
        [NotMapped]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public string Permissions { get; set; }

        // PageModule properties
        [NotMapped]
        public int PageModuleId { get; set; }
        [NotMapped]
        public int PageId { get; set; }
        [NotMapped]
        public string Title { get; set; }
        [NotMapped]
        public string Pane { get; set; }
        [NotMapped]
        public int Order { get; set; }
        [NotMapped]
        public string ContainerType { get; set; }

        // SiteRouter properties
        [NotMapped]
        public string ModuleType { get; set; }
        [NotMapped]
        public int PaneModuleIndex { get; set; }
        [NotMapped]
        public int PaneModuleCount { get; set; }

        // ModuleDefinition
        [NotMapped]
        public ModuleDefinition ModuleDefinition { get; set; }

        // IModuleControl properties
        [NotMapped]
        public SecurityAccessLevel SecurityAccessLevel { get; set; }
        [NotMapped]
        public string ControlTitle { get; set; }
        [NotMapped]
        public string Actions { get; set; }
        [NotMapped]
        public bool UseAdminContainer { get; set; }
    }
}
