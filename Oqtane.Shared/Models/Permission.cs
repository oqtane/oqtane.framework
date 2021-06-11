using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Permission information for anything in Oqtane.
    /// Things in Oqtane are identified as Entities, so anything that can be identified can be described here. 
    /// </summary>
    public class Permission : IAuditable
    {
        /// <summary>
        /// Internal ID storing this information.
        /// </summary>
        public int PermissionId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/> which contains both the target Entity and permissions. 
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Name of the Entity these permissions apply to. 
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// ID of the Entity these permissions apply to. 
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// What this permission is called.
        /// TODO: todoc - must clarify what exactly this means, I assume any module can give it's own names for Permissions
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// <see cref="Role"/> this permission applies to. So if all users in the Role _Customers_ have this permission, then it would reference that Role.
        /// If null, then the permission doesn't target a role but probably a <see cref="User"/> (see <see cref="UserId"/>).
        /// </summary>
        public int? RoleId { get; set; }


        /// <summary>
        /// <see cref="User"/> this permission applies to.  
        /// If null, then the permission doesn't target a User but probably a <see cref="Role"/> (see <see cref="RoleId"/>).
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Determines if Authorization is sufficient to receive this permission.
        /// </summary>
        public bool IsAuthorized { get; set; }

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
        /// Reference to the <see cref="Role"/> based on the <see cref="RoleId"/> - can be nullable.
        /// </summary>
        /// <remarks>
        /// It's not certain if this will always be populated. TODO: todoc/verify
        /// </remarks>
        public Role Role { get; set; }

        public Permission()
        {
        }

        public Permission(string permissionName, string roleName, bool isAuthorized)
        {
            PermissionName = permissionName;
            Role = new Role { Name = roleName };
            IsAuthorized = isAuthorized;
        }

        public Permission(string permissionName, int userId, bool isAuthorized)
        {
            PermissionName = permissionName;
            UserId = userId;
            IsAuthorized = isAuthorized;
        }
    }
}
