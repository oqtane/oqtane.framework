using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Security Role in Oqtane.
    /// </summary>
    public class Role : IAuditable
    {
        /// <summary>
        /// Primary ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Reference to a <see cref="Site"/> if applicable.
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Role name to show in Admin dialogs.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Brief description for Admin dialogs.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines if users automatically get assigned to this role.
        /// </summary>
        public bool IsAutoAssigned { get; set; }
        public bool IsSystem { get; set; }

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
    }
}
