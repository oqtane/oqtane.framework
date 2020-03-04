using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<UserRole> GetUserRoles(int SiteId)
        {
            return _db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == SiteId || item.Role.SiteId == null);
        }

        public IEnumerable<UserRole> GetUserRoles(int UserId, int SiteId)
        {
            return _db.UserRole.Where(item => item.UserId == UserId)
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == SiteId || item.Role.SiteId == null);
        }

        public UserRole AddUserRole(UserRole UserRole)
        {
            _db.UserRole.Add(UserRole);
            _db.SaveChanges();
            return UserRole;
        }

        public UserRole UpdateUserRole(UserRole UserRole)
        {
            _db.Entry(UserRole).State = EntityState.Modified;
            _db.SaveChanges();
            return UserRole;
        }

        public UserRole GetUserRole(int UserRoleId)
        {
            return _db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .SingleOrDefault(item => item.UserRoleId == UserRoleId);
        }

        public void DeleteUserRole(int UserRoleId)
        {
            UserRole UserRole = _db.UserRole.Find(UserRoleId);
            _db.UserRole.Remove(UserRole);
            _db.SaveChanges();
        }
    }
}
