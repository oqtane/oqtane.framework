using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Oqtane.Models;

namespace Oqtane.Extensions
{
    public static class PermissionExtension
    {
        public static string EncodePermissions(this IEnumerable<Permission> permissionList)
        {
            List<PermissionString> permissionstrings = new List<PermissionString>();
            string permissionname = "";
            string permissions = "";
            StringBuilder permissionsbuilder = new StringBuilder();
            string securityid = "";
            foreach (Permission permission in permissionList.OrderBy(item => item.PermissionName))
            {
                // permission collections are grouped by permissionname
                if (permissionname != permission.PermissionName)
                {
                    permissions = permissionsbuilder.ToString();
                    if (permissions != "")
                    {
                        permissionstrings.Add(new PermissionString { PermissionName = permissionname, Permissions = permissions.Substring(0, permissions.Length - 1) });
                    }
                    permissionname = permission.PermissionName;
                    permissionsbuilder = new StringBuilder();
                }

                // deny permissions are prefixed with a "!"
                string prefix = !permission.IsAuthorized ? "!" : "";

                // encode permission
                if (permission.UserId == null)
                {
                    securityid = prefix + permission.Role.Name + ";";
                }
                else
                {
                    securityid = prefix + "[" + permission.UserId + "];";
                }

                // insert deny permissions at the beginning and append grant permissions at the end
                if (prefix == "!")
                {
                    permissionsbuilder.Insert(0, securityid);
                }
                else
                {
                    permissionsbuilder.Append(securityid);
                }
            }

            permissions = permissionsbuilder.ToString();
            if (permissions != "")
            {
                permissionstrings.Add(new PermissionString { PermissionName = permissionname, Permissions = permissions.Substring(0, permissions.Length - 1) });
            }
            return JsonSerializer.Serialize(permissionstrings);
        }
    }
}
