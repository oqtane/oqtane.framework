using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetRoles();
        IEnumerable<Role> GetRoles(int SiteId);
        IEnumerable<Role> GetRoles(int SiteId, bool IncludeGlobalRoles);
        Role AddRole(Role Role);
        Role UpdateRole(Role Role);
        Role GetRole(int RoleId);
        void DeleteRole(int RoleId);
    }
}
