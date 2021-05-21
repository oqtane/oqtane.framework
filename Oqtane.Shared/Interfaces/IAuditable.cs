using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Basic create/edit information - used in many objects. 
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// Username of the creator of this Object.
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Created Timestamp for this Object.
        /// </summary>
        DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Username of the last user who modified this Object.
        /// </summary>
        string ModifiedBy { get; set; }

        /// <summary>
        /// Modified Timestamp for this Object.
        /// </summary>
        DateTime ModifiedOn { get; set; }
    }
}
