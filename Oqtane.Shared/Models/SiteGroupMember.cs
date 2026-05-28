using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class SiteGroupMember : ModelBase
    {
        /// <summary>
        /// ID to identify the site group
        /// </summary>
        public int SiteGroupMemberId { get; set; }

        /// <summary>
        /// Reference to the <see cref="SiteGroup"/>.
        /// </summary>
        public int SiteGroupId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// The last date/time the site was synchronized
        /// </summary>
        public DateTime? SynchronizedOn { get; set; }

        /// <summary>
        /// The <see cref="SiteGroup"/> itself.
        /// </summary>
        public SiteGroup SiteGroup { get; set; }

        /// <summary>
        /// The primary alias for the site 
        /// </summary>
        [NotMapped]
        public string AliasName { get; set; }
    }
}
