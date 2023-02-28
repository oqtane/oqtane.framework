using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Extensions
{
    public static class PermissionExtension
    {
        public static List<Permission> EncodePermissions(this IEnumerable<Permission> permissionList)
        {
            return permissionList.ToList();
        }
    }
}
