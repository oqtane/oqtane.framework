using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Text;
using System;

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
            try
            {
                return db.Permission.Where(item => item.SiteId == SiteId)
                    .Where(item => item.EntityName == EntityName)
                    .Include(item => item.Role); // eager load roles
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Permission> GetPermissions(string EntityName, int EntityId)
        {
            try
            {
                return db.Permission.Where(item => item.EntityName == EntityName)
                    .Where(item => item.EntityId == EntityId)
                    .Include(item => item.Role); // eager load roles
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Permission> GetPermissions(string EntityName, int EntityId, string PermissionName)
        {
            try
            {
                return db.Permission.Where(item => item.EntityName == EntityName)
                    .Where(item => item.EntityId == EntityId)
                    .Where(item => item.PermissionName == PermissionName)
                    .Include(item => item.Role); // eager load roles
            }
            catch
            {
                throw;
            }
        }

        public Permission AddPermission(Permission Permission)
        {
            try
            {
                db.Permission.Add(Permission);
                db.SaveChanges();
                return Permission;
            }
            catch
            {
                throw;
            }
        }

        public Permission UpdatePermission(Permission Permission)
        {
            try
            {
                db.Entry(Permission).State = EntityState.Modified;
                db.SaveChanges();
                return Permission;
            }
            catch
            {
                throw;
            }
        }

        public void UpdatePermissions(int SiteId, string EntityName, int EntityId, string Permissions)
        {
            // get current permissions and delete
            List<Permission> permissions = db.Permission.Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId).ToList();
            foreach(Permission permission in permissions)
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
            try
            {
                return db.Permission.Find(PermissionId);
            }
            catch
            {
                throw;
            }
        }

        public void DeletePermission(int PermissionId)
        {
            try
            {
                Permission Permission = db.Permission.Find(PermissionId);
                db.Permission.Remove(Permission);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        // permissions are stored in the format "{permissionname:!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]}" where "!" designates Deny permissions
        public string EncodePermissions(int EntityId, List<Permission> Permissions)
        {
            string permissions = "";
            string permissionname = "";
            StringBuilder permissionsbuilder = new StringBuilder();
            string perm = "";
            foreach (Permission permission in Permissions.Where(item => item.EntityId == EntityId).OrderBy(item => item.PermissionName))
            {
                // permission collections are grouped by permissionname
                if (permissionname != permission.PermissionName)
                {
                    permissionname = permission.PermissionName;
                    permissions += permissionsbuilder.ToString();
                    permissions += ((permissions != "") ? "}" : "") + "{" + permissionname + ":";
                    permissionsbuilder = new StringBuilder();
                }

                // deny permissions are prefixed with a "!"
                string prefix = !permission.IsAuthorized ? "!" : "";

                // encode permission
                if (permission.UserId == null)
                {
                    perm = prefix + permission.Role.Name + ";";
                }
                else
                {
                    perm = prefix + "[" + permission.UserId.ToString() + "];";
                }

                // insert Deny permissions at the beginning and append Grant permissions at the end
                if (prefix == "!")
                {
                    permissionsbuilder.Insert(0, perm);
                }
                else
                {
                    permissionsbuilder.Append(perm);
                }
            }

            if (permissionsbuilder.ToString() != "")
            {
                permissions += permissionsbuilder.ToString() + "}";
            }

            return permissions;
        }

        public List<Permission> DecodePermissions(string Permissions, int SiteId, string EntityName, int EntityId)
        {
            List<Role> roles = Roles.GetRoles(SiteId).ToList();
            List<Permission> permissions = new List<Permission>();
            string perm = "";
            string permissionname;
            string permissionstring;
            foreach (string PermissionString in Permissions.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                permissionname = PermissionString.Substring(0, PermissionString.IndexOf(":"));
                permissionstring = PermissionString.Replace(permissionname + ":", "").Replace("}", "");
                foreach (string Perm in permissionstring.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    perm = Perm;
                    Permission permission = new Permission();
                    permission.SiteId = SiteId;
                    permission.EntityName = EntityName;
                    permission.EntityId = EntityId;
                    permission.PermissionName = permissionname;
                    permission.RoleId = null;
                    permission.UserId = null;
                    permission.IsAuthorized = true;

                    if (perm.StartsWith("!"))
                    {
                        // deny permission
                        perm.Replace("!", "");
                        permission.IsAuthorized = false;
                    }
                    if (perm.StartsWith("[") && perm.EndsWith("]"))
                    {
                        // user id
                        perm = perm.Replace("[", "").Replace("]", "");
                        permission.UserId = int.Parse(perm);
                    }
                    else
                    {
                        // role name
                        Role role = roles.Where(item => item.Name == perm).SingleOrDefault();
                        if (role != null)
                        {
                            permission.RoleId = role.RoleId;
                        }
                    }
                    permissions.Add(permission);
                }
            }
            return permissions;
        }
    }
}
