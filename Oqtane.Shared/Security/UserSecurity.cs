using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class UserSecurity
    {
        public static bool IsAuthorized(User user, string roles)
        {
            var permissions = new List<Permission>();
            foreach (var role in roles.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                permissions.Add(new Permission("", role, true));
            }
            return IsAuthorized(user, permissions);
        }

        public static bool IsAuthorized(User user, string permissionName, List<Permission> permissions)
        {
            return IsAuthorized(user, permissions.Where(item => item.PermissionName == permissionName).ToList());
        }

        public static bool IsAuthorized(User user, string permissionName, string permissions)
        {
            return IsAuthorized(user, JsonSerializer.Deserialize<List<Permission>>(permissions).Where(item => item.PermissionName == permissionName).ToList());
        }

        public static bool IsAuthorized(User user, List<Permission> permissions)
        {
            bool authorized = false;
            if (permissions != null && permissions.Any())
            {
                if (user == null)
                {
                    authorized = IsAuthorized(-1, "", permissions); // user is not authenticated but may have access to resource
                }
                else
                {
                    authorized = IsAuthorized(user.UserId, user.Roles, permissions);
                }

            }
            return authorized;
        }

        private static bool IsAuthorized(int userId, string roles, List<Permission> permissions)
        {
            bool isAuthorized = false;

            if (permissions != null && permissions.Any())
            {
                // check if denied first
                isAuthorized = !permissions.Where(item => !item.IsAuthorized && (
                    (item.Role != null && (
                        (item.Role.Name == RoleNames.Everyone) ||
                        (item.Role.Name == RoleNames.Unauthenticated && userId == -1) ||
                        roles.Split(';', StringSplitOptions.RemoveEmptyEntries).Contains(item.Role.Name))) ||
                    (item.UserId != null && item.UserId.Value == userId))).Any();

                if (isAuthorized)
                {
                    // then check if authorized
                    isAuthorized = permissions.Where(item => item.IsAuthorized && (
                        (item.Role != null && (
                            (item.Role.Name == RoleNames.Everyone) ||
                            (item.Role.Name == RoleNames.Unauthenticated && userId == -1) ||
                            roles.Split(';', StringSplitOptions.RemoveEmptyEntries).Contains(item.Role.Name))) ||
                        (item.UserId != null && item.UserId.Value == userId))).Any();
                }
            }

            return isAuthorized;
        }

        public static bool ContainsRole(List<Permission> permissions, string permissionName, string roleName)
        {
            return permissions.Any(item => item.PermissionName == permissionName && item.Role.Name == roleName);
        }

        public static bool ContainsUser(List<Permission> permissions, string permissionName, int userId)
        {
            return permissions.Any(item => item.PermissionName == permissionName && item.UserId == userId);
        }

        public static ClaimsIdentity CreateClaimsIdentity(Alias alias, User user, List<UserRole> userroles)
        {
            user.Roles = "";
            foreach (UserRole userrole in userroles)
            {
                user.Roles += userrole.Role.Name + ";";
            }
            if (user.Roles != "") user.Roles = ";" + user.Roles;
            return CreateClaimsIdentity(alias, user);
        }

        public static ClaimsIdentity CreateClaimsIdentity(Alias alias, User user)
        {
            ClaimsIdentity identity = new ClaimsIdentity(Constants.AuthenticationScheme);
            if (alias != null && user != null && !user.IsDeleted)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                identity.AddClaim(new Claim("sitekey", alias.SiteKey));
                if (user.Roles.Contains(RoleNames.Host))
                {
                    // host users are site admins by default
                    identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Host));
                    identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Admin));
                    identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Registered));
                }
                foreach (string role in user.Roles.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!identity.Claims.Any(item => item.Type == ClaimTypes.Role && item.Value == role))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            return identity;
        }
    }
}
