using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUserRoleRepository
    {
        IEnumerable<UserRole> GetUserRoles(int siteId);
        IEnumerable<UserRole> GetUserRoles(int userId, int siteId);
        IEnumerable<UserRole> GetUserRoles(string roleName, int siteId);
        UserRole AddUserRole(UserRole userRole);
        UserRole UpdateUserRole(UserRole userRole);
        UserRole GetUserRole(int userRoleId);
        UserRole GetUserRole(int userRoleId, bool tracking);
        UserRole GetUserRole(int userId, int roleId);
        UserRole GetUserRole(int userId, int roleId, bool tracking);
        void DeleteUserRole(int userRoleId);
        void DeleteUserRoles(int userId);
    }
}
