using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRoleRepository
    {
        IEnumerable<UserRole> GetUserRoles(int siteId);
        IEnumerable<UserRole> GetUserRoles(int userId, int siteId);
        UserRole AddUserRole(UserRole userRole);
        UserRole UpdateUserRole(UserRole userRole);
        UserRole GetUserRole(int userRoleId);
        void DeleteUserRole(int userRoleId);
    }
}
