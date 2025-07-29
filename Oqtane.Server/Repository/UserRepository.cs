using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Modules.Admin.Users;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IFolderRepository _folders;
        private readonly IRoleRepository _roles;
        private readonly IUserRoleRepository _userroles;

        public UserRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IFolderRepository folders, IRoleRepository roles, IUserRoleRepository userroles)
        {
            _dbContextFactory = dbContextFactory;
            _folders = folders;
            _roles = roles;
            _userroles = userroles;
        }
            
        public IEnumerable<User> GetUsers()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.User.ToList();
        }

        public User AddUser(User user)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (db.User.AsNoTracking().FirstOrDefault(item => item.Username == user.Username) == null)
            {
                db.User.Add(user);
                db.SaveChanges();
            }
            else
            {
                int siteId = user.SiteId;
                user = db.User.AsNoTracking().First(item => item.Username == user.Username);
                user.SiteId = siteId;
            }

            // add folder for user
            var folder = _folders.GetFolder(user.SiteId, "Users/");
            if (folder != null)
            {
                _folders.AddFolder(new Folder
                {
                    SiteId = folder.SiteId,
                    ParentId = folder.FolderId,
                    Name = "My Folder",
                    Type = FolderTypes.Private,
                    Path = $"Users/{user.UserId}/",
                    Order = 1,
                    ImageSizes = "",
                    Capacity = Constants.UserFolderCapacity,
                    IsSystem = true,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.Browse, user.UserId, true),
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.Edit, user.UserId, true)
                    }
                });
            }

            // add auto assigned roles to user for site
            var roles = _roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned).ToList();
            foreach (var role in roles)
            {
                var userrole = new UserRole();
                userrole.UserId = user.UserId;
                userrole.RoleId = role.RoleId;
                userrole.EffectiveDate = null;
                userrole.ExpiryDate = null;
                userrole.IgnoreSecurityStamp = true;
                _userroles.AddUserRole(userrole);
            }

            return user;
        }

        public User UpdateUser(User user)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return user;
        }

        public User GetUser(int userId)
        {
            return GetUser(userId, true);
        }

        public User GetUser(int userId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.User.Find(userId);
            }
            else
            {
                return db.User.AsNoTracking().FirstOrDefault(item => item.UserId == userId);
            }
        }

        public User GetUser(string username)
        {
            return GetUser(username, "");
        }

        public User GetUser(string username, string email)
        {
            using var db = _dbContextFactory.CreateDbContext();
            User user = null;
            if (!string.IsNullOrEmpty(username))
            {
                user = db.User.Where(item => item.Username == username).FirstOrDefault();
            }
            if (user == null && !string.IsNullOrEmpty(email))
            {
                user = db.User.Where(item => item.Email == email).FirstOrDefault();
            }
            return user;
        }

        public void DeleteUser(int userId)
        {
            using var db = _dbContextFactory.CreateDbContext();

            // remove permissions for user
            foreach (var permission in db.Permission.Where(item => item.UserId == userId))
            {
                db.Permission.Remove(permission);
            }

            var user = db.User.Find(userId);
            db.User.Remove(user);
            db.SaveChanges();
        }
    }
}
