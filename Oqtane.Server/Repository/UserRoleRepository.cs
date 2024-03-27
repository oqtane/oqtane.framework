using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IRoleRepository _roles;

        public UserRoleRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IRoleRepository roles)
        {
            _dbContextFactory = dbContextFactory;
            _roles = roles;
        }

        public IEnumerable<UserRole> GetUserRoles(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null).ToList();
        }

        public IEnumerable<UserRole> GetUserRoles(int userId, int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.UserRole.Where(item => item.UserId == userId)
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null || siteId == -1).ToList();
        }

        public UserRole AddUserRole(UserRole userRole)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.UserRole.Add(userRole);
            db.SaveChanges();

            // host roles can only exist at global level - remove any site specific user roles
            var role = _roles.GetRole(userRole.RoleId);
            if (role.Name == RoleNames.Host)
            {
                DeleteUserRoles(userRole.UserId);
            }

            return userRole;
        }

        public UserRole UpdateUserRole(UserRole userRole)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(userRole).State = EntityState.Modified;
            db.SaveChanges();
            return userRole;
        }

        public UserRole GetUserRole(int userRoleId)
        {
            return GetUserRole(userRoleId, true);
        }

        public UserRole GetUserRole(int userRoleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
            else
            {
                return db.UserRole.AsNoTracking()
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
        }

        public UserRole GetUserRole(int userId, int roleId)
        {
            return GetUserRole(userId, roleId, true);
        }

        public UserRole GetUserRole(int userId, int roleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserId == userId && item.RoleId == roleId);
            }
            else
            {
                return db.UserRole.AsNoTracking()
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserId == userId && item.RoleId == roleId);
            }
        }

        public void DeleteUserRole(int userRoleId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var userRole = db.UserRole.Find(userRoleId);
            db.UserRole.Remove(userRole);
            db.SaveChanges();
        }

        public void DeleteUserRoles(int userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            foreach (var userRole in db.UserRole.Where(item => item.UserId == userId && item.Role.SiteId != null))
            {
                db.UserRole.Remove(userRole);
            }
            db.SaveChanges();
        }
    }
}
