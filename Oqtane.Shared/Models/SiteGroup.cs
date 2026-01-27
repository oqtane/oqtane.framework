using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class SiteGroup : ModelBase
    {
        /// <summary>
        /// ID to identify the site group
        /// </summary>
        public int SiteGroupId { get; set; }

        /// <summary>
        /// Reference to the <see cref="SiteGroupDefinition"/>.
        /// </summary>
        public int SiteGroupDefinitionId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Indicates if content should be synchronized for the site (false = compare, true = update)
        /// </summary>
        public bool Synchronize { get; set; }

        /// <summary>
        /// The role who should be notified of changes to the site (non-primary site only)
        /// </summary>
        public string NotifyRoleName { get; set; }

        /// <summary>
        /// The last date/time the site was synchronized
        /// </summary>
        public DateTime? SynchronizedOn { get; set; }

        /// <summary>
        /// The <see cref="SiteGroupDefinition"/> itself.
        /// </summary>
        public SiteGroupDefinition SiteGroupDefinition { get; set; }

        /// <summary>
        /// The primary alias for the site 
        /// </summary>
        [NotMapped]
        public string AliasName { get; set; }
    }
}
