using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private TenantDBContext db;

        public UserRoleRepository(TenantDBContext context)
        {
            db = context;
        }

        public IEnumerable<UserRole> GetUserRoles()
        {
            try
            {
                return db.UserRole.ToList();
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<UserRole> GetUserRoles(int UserId)
        {
            try
            {
                return db.UserRole.Where(item => item.UserId == UserId)
                    .Include(item => item.Role) // eager load roles
                    .ToList();
            }
            catch
            {
                throw;
            }
        }

        public UserRole AddUserRole(UserRole UserRole)
        {
            try
            {
                db.UserRole.Add(UserRole);
                db.SaveChanges();
                return UserRole;
            }
            catch
            {
                throw;
            }
        }

        public UserRole UpdateUserRole(UserRole UserRole)
        {
            try
            {
                db.Entry(UserRole).State = EntityState.Modified;
                db.SaveChanges();
                return UserRole;
            }
            catch
            {
                throw;
            }
        }

        public UserRole GetUserRole(int UserRoleId)
        {
            try
            {
                return db.UserRole.Include(item => item.Role) // eager load roles
                    .SingleOrDefault(item => item.UserRoleId == UserRoleId);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteUserRole(int UserRoleId)
        {
            try
            {
                UserRole UserRole = db.UserRole.Find(UserRoleId);
                db.UserRole.Remove(UserRole);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
