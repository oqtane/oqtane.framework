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
            return db.Role;
        }

        public IEnumerable<Role> GetRoles(int SiteId)
        {
            return db.Role.Where(item => item.SiteId == SiteId);
        }

        public IEnumerable<Role> GetRoles(int SiteId, bool IncludeGlobalRoles)
        {
            return db.Role.Where(item => item.SiteId == SiteId || item.SiteId == null);
        }


        public Role AddRole(Role Role)
        {
            db.Role.Add(Role);
            db.SaveChanges();
            return Role;
        }

        public Role UpdateRole(Role Role)
        {
            db.Entry(Role).State = EntityState.Modified;
            db.SaveChanges();
            return Role;
        }

        public Role GetRole(int RoleId)
        {
            return db.Role.Find(RoleId);
        }

        public void DeleteRole(int RoleId)
        {
            Role Role = db.Role.Find(RoleId);
            db.Role.Remove(Role);
            db.SaveChanges();
        }
    }
}
