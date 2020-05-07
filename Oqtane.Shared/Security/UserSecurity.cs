using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class UserSecurity
    {
        public static List<PermissionString> GetPermissionStrings(string permissionStrings)
        {
            return JsonSerializer.Deserialize<List<PermissionString>>(permissionStrings);
        }

        public static string SetPermissionStrings(List<PermissionString> permissionStrings)
        {
            return JsonSerializer.Serialize(permissionStrings);
        }

        public static string GetPermissions(string permissionName, string permissionStrings)
        {
            string permissions = "";
            List<PermissionString> permissionstrings = JsonSerializer.Deserialize<List<PermissionString>>(permissionStrings);
            PermissionString permissionstring = permissionstrings.FirstOrDefault(item => item.PermissionName == permissionName);
            if (permissionstring != null)
            {
                permissions = permissionstring.Permissions;
            }
            return permissions;
        }

        public static bool IsAuthorized(User user, string permissionName, string permissionStrings)
        {
            return IsAuthorized(user, GetPermissions(permissionName, permissionStrings));
        }

        // permissions are stored in the format "!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]" where "!" designates Deny permissions
        public static bool IsAuthorized(User user, string permissions)
        {
            bool authorized = false;
            if (permissions != "")
            {
                if (user == null)
                {
                    authorized =  IsAuthorized(-1, "", permissions); // user is not authenticated but may have access to resource
                }
                else
                {
                    authorized = IsAuthorized(user.UserId, user.Roles, permissions);
                }

            }
            return authorized;
        }

        private static bool IsAuthorized(int userId, string roles, string permissions)
        {
            bool isAuthorized = false;

            if (permissions != null)
            {
                foreach (string permission in permissions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    bool? allowed = VerifyPermission(userId, roles, permission);
                    if (allowed.HasValue)
                    {
                        isAuthorized = allowed.Value;
                        break;
                    }
                }
            }

            return isAuthorized;
        }

        private static bool? VerifyPermission(int userId, string roles, string permission)
        {
            bool? allowed = null;
            //permissions strings are encoded with deny permissions at the beginning and grant permissions at the end for optimal performance
            if (!String.IsNullOrEmpty(permission))
            {
                // deny permission
                if (permission.StartsWith("!"))
                {
                    string denyRole = permission.Replace("!", "");
                    if (denyRole == Constants.AllUsersRole || IsAllowed(userId, roles, denyRole))
                    {
                        allowed = false;
                    }
                }
                else // grant permission
                {
                    if (permission == Constants.AllUsersRole || IsAllowed(userId, roles, permission))
                    {
                        allowed = true;
                    }
                }
            }
            return allowed;
        }

        private static bool IsAllowed(int userId, string roles, string permission)
        {
            if ("[" + userId + "]" == permission)
            {
                return true;
            }

            if (roles != null)
            {
                return roles.IndexOf(";" + permission + ";") != -1;
            }
            return false;
        }
    }
}
