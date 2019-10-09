using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private TenantDBContext db;
        private readonly IRoleRepository Roles;

        public PermissionRepository(TenantDBContext context, IRoleRepository Roles)
        {
            db = context;
            this.Roles = Roles;
        }

        public IEnumerable<Permission> GetPermissions(int SiteId, string EntityName)
        {
            return db.Permission.Where(item => item.SiteId == SiteId)
                .Where(item => item.EntityName == EntityName)
                .Include(item => item.Role); // eager load roles
        }

        public IEnumerable<Permission> GetPermissions(string EntityName, int EntityId)
        {
            return db.Permission.Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId)
                .Include(item => item.Role); // eager load roles
        }

        public IEnumerable<Permission> GetPermissions(string EntityName, int EntityId, string PermissionName)
        {
            return db.Permission.Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId)
                .Where(item => item.PermissionName == PermissionName)
                .Include(item => item.Role); // eager load roles
        }

        public Permission AddPermission(Permission Permission)
        {
            db.Permission.Add(Permission);
            db.SaveChanges();
            return Permission;
        }

        public Permission UpdatePermission(Permission Permission)
        {
            db.Entry(Permission).State = EntityState.Modified;
            db.SaveChanges();
            return Permission;
        }

        public void UpdatePermissions(int SiteId, string EntityName, int EntityId, string Permissions)
        {
            // get current permissions and delete
            IEnumerable<Permission> permissions = db.Permission
                .Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId)
                .Where(item => item.SiteId == SiteId);
            foreach (Permission permission in permissions)
            {
                db.Permission.Remove(permission);
            }
            // add permissions
            permissions = DecodePermissions(Permissions, SiteId, EntityName, EntityId);
            foreach (Permission permission in permissions)
            {
                db.Permission.Add(permission);
            }
            db.SaveChanges();
        }

        public Permission GetPermission(int PermissionId)
        {
            return db.Permission.Find(PermissionId);
        }

        public void DeletePermission(int PermissionId)
        {
            Permission Permission = db.Permission.Find(PermissionId);
            db.Permission.Remove(Permission);
            db.SaveChanges();
        }

        public void DeletePermissions(int SiteId, string EntityName, int EntityId)
        {
            IEnumerable<Permission> permissions = db.Permission
                .Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId)
                .Where(item => item.SiteId == SiteId);
            foreach (Permission permission in permissions)
            {
                db.Permission.Remove(permission);
            }
            db.SaveChanges();
        }

        // permissions are stored in the format "{permissionname:!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]}" where "!" designates Deny permissions
        public string EncodePermissions(int EntityId, IEnumerable<Permission> Permissions)
        {
            List<PermissionString> permissionstrings = new List<PermissionString>();
            string permissionname = "";
            string permissions = "";
            StringBuilder permissionsbuilder = new StringBuilder();
            string securityid = "";
            foreach (Permission permission in Permissions.Where(item => item.EntityId == EntityId).OrderBy(item => item.PermissionName))
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
                    securityid = prefix + "[" + permission.UserId.ToString() + "];";
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

        public IEnumerable<Permission> DecodePermissions(string PermissionStrings, int SiteId, string EntityName, int EntityId)
        {
            List<Permission> permissions = new List<Permission>();
            List<Role> roles = Roles.GetRoles(SiteId, true).ToList();
            string securityid = "";
            foreach (PermissionString permissionstring in JsonSerializer.Deserialize<List<PermissionString>>(PermissionStrings))
            {
                foreach (string id in permissionstring.Permissions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    securityid = id;
                    Permission permission = new Permission();
                    permission.SiteId = SiteId;
                    permission.EntityName = EntityName;
                    permission.EntityId = EntityId;
                    permission.PermissionName = permissionstring.PermissionName;
                    permission.RoleId = null;
                    permission.UserId = null;
                    permission.IsAuthorized = true;

                    if (securityid.StartsWith("!"))
                    {
                        // deny permission
                        securityid.Replace("!", "");
                        permission.IsAuthorized = false;
                    }
                    if (securityid.StartsWith("[") && securityid.EndsWith("]"))
                    {
                        // user id
                        securityid = securityid.Replace("[", "").Replace("]", "");
                        permission.UserId = int.Parse(securityid);
                    }
                    else
                    {
                        // role name
                        Role role = roles.Where(item => item.Name == securityid).SingleOrDefault();
                        if (role != null)
                        {
                            permission.RoleId = role.RoleId;
                        }
                    }
                    if (permission.UserId != null || permission.RoleId != null)
                    {
                        permissions.Add(permission);
                    }
                }
            }
            return permissions;
        }
    }
}
