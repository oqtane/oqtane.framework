using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Sync
    {
        public DateTime SyncDate { get; set; }
        public List<SyncEvent> SyncEvents { get; set; }
    }

    public class SyncEvent : EventArgs
    {
        public int TenantId { get; set; }
        public int SiteId { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// Unique key used for identifying a site within a runtime process (ie. cache, file system, etc...)
        /// </summary>
        [NotMapped]
        public string SiteKey
        {
            get
            {
                return TenantId.ToString() + ":" + SiteId.ToString();
            }
        }
    }
}
