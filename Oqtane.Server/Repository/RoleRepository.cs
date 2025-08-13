using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetRoles(int siteId);
        IEnumerable<Role> GetRoles(int siteId, bool includeGlobalRoles);
        Role AddRole(Role role);
        Role UpdateRole(Role role);
        Role GetRole(int roleId);
        Role GetRole(int roleId, bool tracking);
        void DeleteRole(int roleId);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public RoleRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<Role> GetRoles(int siteId)
        {
            return GetRoles(siteId, false);
        }

        public IEnumerable<Role> GetRoles(int siteId, bool includeGlobalRoles)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (includeGlobalRoles)
            {
                return db.Role.Where(item => item.SiteId == siteId || item.SiteId == null).ToList();
            }
            else
            {
                return db.Role.Where(item => item.SiteId == siteId).ToList();
            }
        }

        public Role AddRole(Role role)
        {
            using var db = _dbContextFactory.CreateDbContext();
            role.Description = role.Description.Substring(0, (role.Description.Length > 256) ? 256 : role.Description.Length);
            db.Role.Add(role);
            db.SaveChanges();
            return role;
        }

        public Role UpdateRole(Role role)
        {
            using var db = _dbContextFactory.CreateDbContext();
            role.Description = role.Description.Substring(0, (role.Description.Length > 256) ? 256 : role.Description.Length);
            db.Entry(role).State = EntityState.Modified;
            db.SaveChanges();
            return role;
        }

        public Role GetRole(int roleId)
        {
            return GetRole(roleId, true);
        }

        public Role GetRole(int roleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.Role.Find(roleId);
            }
            else
            {
                return db.Role.AsNoTracking().FirstOrDefault(item => item.RoleId == roleId);
            }
        }

        public void DeleteRole(int roleId)
        {
            using var db = _dbContextFactory.CreateDbContext();

            // remove userroles for role
            foreach (var userrole in db.UserRole.Where(item => item.RoleId == roleId))
            {
                db.UserRole.Remove(userrole);
            }

            // remove permissions for role
            foreach (var permission in db.Permission.Where(item => item.RoleId == roleId))
            {
                db.Permission.Remove(permission);
            }

            Role role = db.Role.Find(roleId);
            db.Role.Remove(role);
            db.SaveChanges();
        }
    }
}
