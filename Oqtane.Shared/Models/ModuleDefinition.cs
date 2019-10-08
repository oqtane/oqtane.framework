using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class ModuleDefinition : IAuditable
    {
        public int ModuleDefinitionId { get; set; }
        public string ModuleDefinitionName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string Description { get; set; }
        [NotMapped]
        public string Categories { get; set; }
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
        public string PermissionNames { get; set; }
        [NotMapped]
        public string ServerAssemblyName { get; set; }
        [NotMapped]
        public string ControlTypeTemplate { get; set; }
        [NotMapped]
        public string ControlTypeRoutes { get; set; }
        [NotMapped]
        public string AssemblyName { get; set; }
        [NotMapped]
        public int SiteId { get; set; }
        [NotMapped]
        public string Permissions { get; set; }
    }
}
