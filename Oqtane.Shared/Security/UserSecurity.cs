using System;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class UserSecurity
    {
        // permission collections are stored in format {permissionname1:permissions}{permissionname2:permissions}...
        public static string GetPermissions(string PermissionName, string Permissions)
        {
            string permissions = "";
            foreach(string permission in Permissions.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (permission.StartsWith(PermissionName + ":"))
                {
                    permissions = permission.Replace(PermissionName + ":", "").Replace("}", "");
                    break;
                }
            }
            return permissions;
        }

        public static string SetPermissions(string PermissionName, string Permissions)
        {
            return "{" + PermissionName + ":" + Permissions + "}";
        }

        // permissions are stored in the format "!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]" where "!" designates Deny permissions
        public static bool IsAuthorized(User User, string PermissionName, string Permissions)
        {
            Permissions = GetPermissions(PermissionName, Permissions);
            if (User == null)
            {
                return IsAuthorized(-1, "", Permissions); // user is not authenticated but may have access to resource
            }
            else
            {
                return IsAuthorized(User.UserId, User.Roles, Permissions);
            }
        }

        private static bool IsAuthorized(int UserId, string Roles, string Permissions)
        {
            bool IsAuthorized = false;

            if (Permissions != null)
            {
                foreach (string permission in Permissions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    bool? allowed = VerifyPermission(UserId, Roles, permission);
                    if (allowed.HasValue)
                    {
                        IsAuthorized = allowed.Value;
                        break;
                    }
                }
            }

            return IsAuthorized;
        }

        private static bool? VerifyPermission(int UserId, string Roles, string Permission)
        {
            bool? allowed = null;
            //permissions strings are encoded with deny permissions at the beginning and grant permissions at the end for optimal performance
            if (!String.IsNullOrEmpty(Permission))
            {
                // deny permission
                if (Permission.StartsWith("!"))
                {
                    string denyRole = Permission.Replace("!", "");
                    if (denyRole == Constants.AllUsersRole || IsAllowed(UserId, Roles, denyRole))
                    {
                        allowed = false;
                    }
                }
                else // grant permission
                {
                    if (Permission == Constants.AllUsersRole || IsAllowed(UserId, Roles, Permission))
                    {
                        allowed = true;
                    }
                }
            }
            return allowed;
        }

        private static bool IsAllowed(int UserId, string Roles, string Permission)
        {
            if ("[" + UserId + "]" == Permission)
            {
                return true;
            }

            if (Roles != null)
            {
                return Roles.IndexOf(";" + Permission + ";") != -1;
            }
            return false;
        }
    }
}
