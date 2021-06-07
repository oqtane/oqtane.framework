using System.Collections.Generic;
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
}
