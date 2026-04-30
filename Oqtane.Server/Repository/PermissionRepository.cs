using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPermissionRepository
    {
        IEnumerable<Permission> GetPermissions(int siteId, string entityName);
        IEnumerable<Permission> GetPermissions(int siteId, string entityName, string permissionName);
        IEnumerable<Permission> GetPermissions(int siteId, string entityName, int entityId);
        IEnumerable<Permission> GetPermissions(int siteId, string entityName, int entityId, string permissionName);
        Permission AddPermission(Permission permission);
        Permission UpdatePermission(Permission permission);
        void UpdatePermissions(int siteId, string entityName, int entityId, List<Permission> permissions);
        Permission GetPermission(int permissionId);
        void DeletePermission(int permissionId);
        void DeletePermissions(int siteId, string entityName, int entityId);
    }

    public class PermissionRepository : IPermissionRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IRoleRepository _roles;
        private readonly ITenantManager _tenantManager;
        private readonly ICacheManager _cache;

        public PermissionRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IRoleRepository roles, ITenantManager tenantManager, ICacheManager cache)
        {
            _dbContextFactory = dbContextFactory;
            _roles = roles;
            _tenantManager = tenantManager;
            _cache = cache;
         }

        public IEnumerable<Permission> GetPermissions(int siteId, string entityName)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return _cache.GetCache(_tenantManager.GetAlias(), $"Permissions:{entityName}", entry =>
            {
                var roles = _roles.GetRoles(siteId, true).ToList();
                var permissions = db.Permission.Where(item => item.SiteId == siteId && item.EntityName == entityName).ToList();
                foreach (var permission in permissions)
                {
                    if (permission.RoleId != null && string.IsNullOrEmpty(permission.RoleName))
                    {
                        permission.RoleName = roles.Find(item => item.RoleId == permission.RoleId)?.Name;
                    }
                }
                permissions = permissions.Where(item => item.UserId != null || item.RoleName != null).ToList();
                return permissions;
            });
        }

        public IEnumerable<Permission> GetPermissions(int siteId, string entityName, string permissionName)
        {
            var permissions = GetPermissions(siteId, entityName);
            return permissions.Where(item => item.PermissionName == permissionName);
        }

        public IEnumerable<Permission> GetPermissions(int siteId, string entityName, int entityId)
        {
            var permissions = GetPermissions(siteId, entityName);
            return permissions.Where(item => item.EntityId == entityId);
        }

        public IEnumerable<Permission> GetPermissions(int siteId, string entityName, int entityId, string permissionName)
        {
            var permissions = GetPermissions(siteId, entityName);
            return permissions.Where(item => item.EntityId == entityId)
                .Where(item => item.PermissionName == permissionName);
        }


        public Permission AddPermission(Permission permission)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Permission.Add(permission);
            db.SaveChanges();
            ClearCache(permission.EntityName);
            return permission;
        }

        public Permission UpdatePermission(Permission permission)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(permission).State = EntityState.Modified;
            db.SaveChanges();
            ClearCache(permission.EntityName);
            return permission;
        }

        public void UpdatePermissions(int siteId, string entityName, int entityId, List<Permission> permissions)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // ensure permissions are fully populated
            var roles = _roles.GetRoles(siteId, true).ToList();
            foreach (var permission in permissions)
            {
                permission.SiteId = siteId;
                permission.EntityName = (string.IsNullOrEmpty(permission.EntityName)) ? entityName : permission.EntityName;
                permission.EntityId = (permission.EntityName == entityName) ? entityId : -1;
                if (permission.UserId == null && permission.RoleId == null && !string.IsNullOrEmpty(permission.RoleName))
                {
                    permission.RoleId = roles.FirstOrDefault(item => item.Name == permission.RoleName)?.RoleId;
                }
            }
            // add or update permissions
            bool modified = false;
            var existing = new List<Permission>();
            foreach (var permission in permissions)
            {
                if (!existing.Any(item => item.EntityName == permission.EntityName && item.PermissionName == permission.PermissionName))
                {
                    existing.AddRange(GetPermissions(siteId, permission.EntityName, permission.PermissionName)
                        .Where(item => item.EntityId == entityId || item.EntityId == -1));
                }

                var current = existing.FirstOrDefault(item => item.EntityName == permission.EntityName && item.EntityId == permission.EntityId
                    && item.PermissionName == permission.PermissionName && item.RoleId == permission.RoleId && item.UserId == permission.UserId);
                if (current != null)
                {
                    if (current.IsAuthorized != permission.IsAuthorized)
                    {
                        current.IsAuthorized = permission.IsAuthorized;
                        db.Entry(current).State = EntityState.Modified;
                        modified = true;
                    }
                }
                else
                {
                    db.Permission.Add(permission);
                    modified = true;
                }
            }
            // delete permissions
            foreach (var permission in existing)
            {
                if (!permissions.Any(item => item.EntityName == permission.EntityName && item.PermissionName == permission.PermissionName
                    && item.EntityId == permission.EntityId && item.RoleId == permission.RoleId && item.UserId == permission.UserId))
                {
                    db.Permission.Remove(permission);
                    modified = true;
                }
            }
            if (modified)
            {
                db.SaveChanges();
                foreach (var entityname in permissions.Select(item => item.EntityName).Distinct())
                {
                    ClearCache(entityname);
                }
            }
        }

        public Permission GetPermission(int permissionId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Permission.Find(permissionId);
        }

        public void DeletePermission(int permissionId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var permission = db.Permission.Find(permissionId);
            db.Permission.Remove(permission);
            db.SaveChanges();
            ClearCache(permission.EntityName);
        }

        public void DeletePermissions(int siteId, string entityName, int entityId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var permissions = db.Permission
                    .Where(item => item.EntityName == entityName)
                    .Where(item => item.EntityId == entityId)
                    .Where(item => item.SiteId == siteId);
            foreach (Permission permission in permissions)
            {
                db.Permission.Remove(permission);
            }
            db.SaveChanges();
            ClearCache(entityName);
        }

        private void ClearCache(string entityName)
        {
            _cache.RemoveCache(_tenantManager.GetAlias(), $"Permissions:{entityName}");
        }
     }
}
