using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Site : IAuditable, IDeletable
    {
        public int SiteId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string DefaultThemeType { get; set; }
        public string DefaultLayoutType { get; set; }
        public string DefaultContainerType { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        [NotMapped]
        public string TenantRootPath
        {
            get
            {
                return "Tenants/" + TenantId.ToString() + "/";
            }
        }

        [NotMapped]
        public string SiteRootPath
        {
            get
            {
                return "Tenants/" + TenantId.ToString() + "/Sites/" + SiteId.ToString() + "/";
            }
        }
    }
}
