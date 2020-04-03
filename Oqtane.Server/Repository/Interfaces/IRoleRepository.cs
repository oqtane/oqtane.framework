using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository.Interfaces
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetRoles(int siteId);
        IEnumerable<Role> GetRoles(int siteId, bool includeGlobalRoles);
        Role AddRole(Role role);
        Role UpdateRole(Role role);
        Role GetRole(int roleId);
        void DeleteRole(int roleId);
    }
}
