using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Permission information for anything in Oqtane.
    /// Things in Oqtane are identified as Entities, so anything that can be identified can be described here. 
    /// </summary>
    public class Permission : ModelBase
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
        /// Name of the Entity these permissions apply to (ie. Module )
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// ID of the Entity these permissions apply to (ie. a ModuleId). A value of -1 indicates the permission applies to all EntityNames regardless of ID (ie. API permissions)
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Name of the permission (ie. View) 
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// <see cref="Role"/> this permission applies to. If null then this is a <see cref="User"/> permission.
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// The role name associated to the RoleId.
        /// </summary>
        [NotMapped]
        public string RoleName { get; set; }

        /// <summary>
        /// <see cref="User"/> this permission applies to. If null then this is a <see cref="Role"/> permission.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// The type of permission (ie. grant = true, deny = false)
        /// </summary>
        public bool IsAuthorized { get; set; }

        public Permission()
        {
        }

        public Permission(string permissionName, string roleName, bool isAuthorized)
        {
            Initialize(-1, "", -1, permissionName, roleName, null, isAuthorized);
        }

        public Permission(string permissionName, int userId, bool isAuthorized)
        {
            Initialize(-1, "", -1, permissionName, "", userId, isAuthorized);
        }

        public Permission(int siteId, string entityName, string permissionName, string roleName, int? userId, bool isAuthorized)
        {
            Initialize(siteId, entityName, -1, permissionName, roleName, userId, isAuthorized);
        }

        public Permission(int siteId, string entityName, int entityId, string permissionName, string roleName, int? userId, bool isAuthorized)
        {
            Initialize(siteId, entityName, entityId, permissionName, roleName, userId, isAuthorized);
        }

        private void Initialize(int siteId, string entityName, int entityId, string permissionName, string roleName, int? userId, bool isAuthorized)
        {
            SiteId = siteId;
            EntityName = entityName;
            EntityId = entityId;
            PermissionName = permissionName;
            if (!string.IsNullOrEmpty(roleName))
            {
                RoleId = null;
                RoleName = roleName;
                UserId = null;
            }
            else
            {
                RoleId = null;
                RoleName = null;
                UserId = userId;
            }
            IsAuthorized = isAuthorized;
        }

        [Obsolete("The Role property is deprecated", false)]
        [NotMapped]
        [JsonIgnore] // exclude from API payload
        public Role Role { get; set; }
    }
}
