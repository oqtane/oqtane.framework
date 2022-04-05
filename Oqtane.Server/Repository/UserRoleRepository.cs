using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private TenantDBContext _db;
        private readonly IRoleRepository _roles;

        public UserRoleRepository(TenantDBContext context, IRoleRepository roles)
        {
            _db = context;
            _roles = roles;
        }

        public IEnumerable<UserRole> GetUserRoles(int siteId)
        {
            return _db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null);
        }

        public IEnumerable<UserRole> GetUserRoles(int userId, int siteId)
        {
            return _db.UserRole.Where(item => item.UserId == userId)
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null || siteId == -1);
        }

        public UserRole AddUserRole(UserRole userRole)
        {
            _db.UserRole.Add(userRole);
            _db.SaveChanges();

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
            _db.Entry(userRole).State = EntityState.Modified;
            _db.SaveChanges();
            return userRole;
        }

        public UserRole GetUserRole(int userRoleId)
        {
            return GetUserRole(userRoleId, true);
        }

        public UserRole GetUserRole(int userRoleId, bool tracking)
        {
            if (tracking)
            {
                return _db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
            else
            {
                return _db.UserRole.AsNoTracking()
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
        }

        public void DeleteUserRole(int userRoleId)
        {
            UserRole userRole = _db.UserRole.Find(userRoleId);
            _db.UserRole.Remove(userRole);
            _db.SaveChanges();
        }

        public void DeleteUserRoles(int userId)
        {
            foreach (UserRole userRole in _db.UserRole.Where(item => item.UserId == userId && item.Role.SiteId != null))
            {
                _db.UserRole.Remove(userRole);
            }
            _db.SaveChanges();
        }
    }
}
