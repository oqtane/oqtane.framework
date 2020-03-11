using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public static class UserSecurity
    {
        public static List<PermissionString> GetPermissionStrings(string PermissionStrings)
        {
            return JsonSerializer.Deserialize<List<PermissionString>>(PermissionStrings);
        }

        public static string SetPermissionStrings(List<PermissionString> PermissionStrings)
        {
            return JsonSerializer.Serialize(PermissionStrings);
        }

        public static string GetPermissions(string PermissionName, string PermissionStrings)
        {
            var permissions = "";
            var permissionStrings = JsonSerializer.Deserialize<List<PermissionString>>(PermissionStrings);
            var permissionString = permissionStrings.FirstOrDefault(item => item.PermissionName == PermissionName);
            if (permissionString != null)
            {
                permissions = permissionString.Permissions;
            }
            return permissions;
        }

        public static bool IsAuthorized(User User, string PermissionName, string PermissionStrings)
        {
            return IsAuthorized(User, GetPermissions(PermissionName, PermissionStrings));
        }

        // permissions are stored in the format "!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]" where "!" designates Deny permissions
        public static bool IsAuthorized(User User, string Permissions)
        {
            var authorized = false;
            if (Permissions != "")
            {
                if (User == null)
                {
                    authorized =  IsAuthorized(-1, "", Permissions); // user is not authenticated but may have access to resource
                }
                else
                {
                    authorized = IsAuthorized(User.UserId, User.Roles, Permissions);
                }

            }
            return authorized;
        }

        private static bool IsAuthorized(int UserId, string Roles, string Permissions)
        {
            var isAuthorized = false;

            if (Permissions != null)
            {
                foreach (var permission in Permissions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var allowed = VerifyPermission(UserId, Roles, permission);
                    if (allowed.HasValue)
                    {
                        isAuthorized = allowed.Value;
                        break;
                    }
                }
            }

            return isAuthorized;
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
                    var denyRole = Permission.Replace("!", "");
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
                return Roles.Contains(";" + Permission + ";");
            }
            return false;
        }
    }
}
