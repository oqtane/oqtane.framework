using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Assigns a <see cref="Role"/> to a <see cref="User"/>
    /// </summary>
    public class UserRole : IAuditable
    {
        /// <summary>
        /// Id of this assignment
        /// </summary>
        public int UserRoleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="User"/> who receives this <see cref="Role"/> assignment.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Role"/> which the <see cref="User"/> receives
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Start of when this assignment is valid. See also <see cref="ExpiryDate"/>
        /// </summary>
        public DateTime? EffectiveDate { get; set; }
        /// <summary>
        /// End of when this assignment is valid. See also <see cref="EffectiveDate"/>
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

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

        /// <summary>
        /// Direct reference to the <see cref="Role"/> object.
        /// TODO: todoc - is this always populated?
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Direct reference to the <see cref="User"/> object.
        /// TODO: todoc - is this always populated?
        /// </summary>
        public User User { get; set; }
    }
}
