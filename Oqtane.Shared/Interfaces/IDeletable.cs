using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Audit information for things than can be deleted.
    /// </summary>
    public interface IDeletable
    {
        /// <summary>
        /// <see cref="User"/> who deleted this object.
        /// </summary>
        string DeletedBy { get; set; }

        /// <summary>
        /// Timestamp when it was deleted.
        /// </summary>
        DateTime? DeletedOn { get; set; }

        /// <summary>
        /// If something is deleted, this will be true.
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
