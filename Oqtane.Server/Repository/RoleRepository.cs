using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private TenantDBContext _db;

        public RoleRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<Role> GetRoles(int SiteId)
        {
            return _db.Role.Where(item => item.SiteId == SiteId);
        }

        public IEnumerable<Role> GetRoles(int SiteId, bool IncludeGlobalRoles)
        {
            return _db.Role.Where(item => item.SiteId == SiteId || item.SiteId == null);
        }


        public Role AddRole(Role Role)
        {
            _db.Role.Add(Role);
            _db.SaveChanges();
            return Role;
        }

        public Role UpdateRole(Role Role)
        {
            _db.Entry(Role).State = EntityState.Modified;
            _db.SaveChanges();
            return Role;
        }

        public Role GetRole(int RoleId)
        {
            return _db.Role.Find(RoleId);
        }

        public void DeleteRole(int RoleId)
        {
            Role Role = _db.Role.Find(RoleId);
            _db.Role.Remove(Role);
            _db.SaveChanges();
        }
    }
}
