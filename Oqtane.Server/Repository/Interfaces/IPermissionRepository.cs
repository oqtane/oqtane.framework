using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPermissionRepository
    {
        IEnumerable<Permission> GetPermissions(int siteId, string entityName);
        IEnumerable<Permission> GetPermissions(string entityName, int entityId);
        IEnumerable<Permission> GetPermissions(string entityName, int entityId, string permissionName);
        Permission AddPermission(Permission permission);
        Permission UpdatePermission(Permission permission);
        void UpdatePermissions(int siteId, string entityName, int entityId, string permissionStrings);
        Permission GetPermission(int permissionId);
        void DeletePermission(int permissionId);
        void DeletePermissions(int siteId, string entityName, int entityId);
        string EncodePermissions(IEnumerable<Permission> permissionList);
        IEnumerable<Permission> DecodePermissions(string permissions, int siteId, string entityName, int entityId);
     }
}
