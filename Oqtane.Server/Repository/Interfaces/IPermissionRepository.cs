using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPermissionRepository
    {
        IEnumerable<Permission> GetPermissions(int SiteId, string EntityName);
        IEnumerable<Permission> GetPermissions(string EntityName, int EntityId);
        IEnumerable<Permission> GetPermissions(string EntityName, int EntityId, string PermissionName);
        Permission AddPermission(Permission Permission);
        Permission UpdatePermission(Permission Permission);
        void UpdatePermissions(int SiteId, string EntityName, int EntityId, string Permissions);
        Permission GetPermission(int PermissionId);
        void DeletePermission(int PermissionId);
        void DeletePermissions(int SiteId, string EntityName, int EntityId);
        string EncodePermissions(int EntityId, IEnumerable<Permission> Permissions);
        IEnumerable<Permission> DecodePermissions(string Permissions, int SiteId, string EntityName, int EntityId);
     }
}
