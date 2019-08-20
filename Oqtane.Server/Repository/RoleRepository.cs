using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private TenantDBContext db;

        public RoleRepository(TenantDBContext context)
        {
            db = context;
        }
            
        public IEnumerable<Role> GetRoles()
        {
            try
            {
                return db.Role.ToList();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Role> GetRoles(int SiteId)
        {
            try
            {
                return db.Role.Where(item => item.SiteId == SiteId).ToList();
            }
            catch
            {
                throw;
            }
        }

        public Role AddRole(Role Role)
        {
            try
            {
                db.Role.Add(Role);
                db.SaveChanges();
                return Role;
            }
            catch
            {
                throw;
            }
        }

        public Role UpdateRole(Role Role)
        {
            try
            {
                db.Entry(Role).State = EntityState.Modified;
                db.SaveChanges();
                return Role;
            }
            catch
            {
                throw;
            }
        }

        public Role GetRole(int RoleId)
        {
            try
            {
                return db.Role.Find(RoleId);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteRole(int RoleId)
        {
            try
            {
                Role Role = db.Role.Find(RoleId);
                db.Role.Remove(Role);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
