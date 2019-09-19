using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRoleRepository
    {
        IEnumerable<UserRole> GetUserRoles();
        IEnumerable<UserRole> GetUserRoles(int SiteId);
        IEnumerable<UserRole> GetUserRoles(int UserId, int SiteId);
        UserRole AddUserRole(UserRole UserRole);
        UserRole UpdateUserRole(UserRole UserRole);
        UserRole GetUserRole(int UserRoleId);
        void DeleteUserRole(int UserRoleId);
    }
}
