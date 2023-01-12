using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Tenant in Oqtane.
    /// Tenants can contain multiple <see cref="Site"/>s and have all their data in a separate Database.
    /// </summary>
    public class Tenant : ModelBase
    {
        /// <summary>
        /// ID of the Tenant.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Name of the Tenant to show in Tenant lists.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Connection string to access this Tenant DB.
        /// </summary>
        public string DBConnectionString { get; set; }

        /// <summary>
        /// Type of DB used in this Tenant
        /// </summary>
        /// <remarks>
        /// New in v2.1.0
        /// </remarks>
        public string DBType { get; set; }
        public string Version { get; set; }       
    }
}
