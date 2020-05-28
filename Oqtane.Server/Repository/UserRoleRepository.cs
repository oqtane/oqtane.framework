using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private TenantDBContext _db;

        public UserRoleRepository(TenantDBContext context)
        {
            _db = context;
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
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null);
        }

        public UserRole AddUserRole(UserRole userRole)
        {
            _db.UserRole.Add(userRole);
            _db.SaveChanges();
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
            return _db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .SingleOrDefault(item => item.UserRoleId == userRoleId);
        }

        public void DeleteUserRole(int userRoleId)
        {
            UserRole userRole = _db.UserRole.Find(userRoleId);
            _db.UserRole.Remove(userRole);
            _db.SaveChanges();
        }
    }
}
