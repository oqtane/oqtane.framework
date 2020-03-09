using System;

namespace Oqtane.Models
{
    public class SyncEvent
    {
        public int TenantId { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
