using System;

namespace Oqtane.Models
{
    public class Permission : IAuditable
    {
        public int PermissionId { get; set; }
        public int SiteId { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string PermissionName { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public bool IsAuthorized { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

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
