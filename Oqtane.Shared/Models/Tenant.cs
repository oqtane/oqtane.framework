using System;

namespace Oqtane.Models
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public string Alias { get; set; }
        public string DBConnectionString { get; set; }
        public string DBSchema { get; set; }
        public int SiteId { get; set; }
    }
}
