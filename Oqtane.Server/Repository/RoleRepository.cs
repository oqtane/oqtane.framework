using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            
        public IEnumerable<Role> GetRoles(int siteId)
        {
            return _db.Role.Where(item => item.SiteId == siteId);
        }

        public IEnumerable<Role> GetRoles(int siteId, bool includeGlobalRoles)
        {
            return _db.Role.Where(item => item.SiteId == siteId || item.SiteId == null);
        }


        public Role AddRole(Role role)
        {
            _db.Role.Add(role);
            _db.SaveChanges();
            return role;
        }

        public Role UpdateRole(Role role)
        {
            _db.Entry(role).State = EntityState.Modified;
            _db.SaveChanges();
            return role;
        }

        public Role GetRole(int roleId)
        {
            return _db.Role.Find(roleId);
        }

        public void DeleteRole(int roleId)
        {
            Role role = _db.Role.Find(roleId);
            _db.Role.Remove(role);
            _db.SaveChanges();
        }
    }
}
