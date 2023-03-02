using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class UserRepository : IUserRepository
    {
        private TenantDBContext _db;
        private readonly IFolderRepository _folders;
        private readonly IRoleRepository _roles;
        private readonly IUserRoleRepository _userroles;

        public UserRepository(TenantDBContext context, IFolderRepository folders, IRoleRepository roles, IUserRoleRepository userroles)
        {
            _db = context;
            _folders = folders;
            _roles = roles;
            _userroles = userroles;
        }
            
        public IEnumerable<User> GetUsers()
        {
            return _db.User;
        }

        public User AddUser(User user)
        {
            if (_db.User.AsNoTracking().FirstOrDefault(item => item.Username == user.Username) == null)
            {
                _db.User.Add(user);
                _db.SaveChanges();
            }
            else
            {
                user = _db.User.AsNoTracking().First(item => item.Username == user.Username);
            }

            // add folder for user
            Folder folder = _folders.GetFolder(user.SiteId, "Users/");
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
            List<Role> roles = _roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned).ToList();
            foreach (Role role in roles)
            {
                UserRole userrole = new UserRole();
                userrole.UserId = user.UserId;
                userrole.RoleId = role.RoleId;
                userrole.EffectiveDate = null;
                userrole.ExpiryDate = null;
                _userroles.AddUserRole(userrole);
            }

            return user;
        }

        public User UpdateUser(User user)
        {
            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();
            return user;
        }

        public User GetUser(int userId)
        {
            return GetUser(userId, true);
        }

        public User GetUser(int userId, bool tracking)
        {
            if (tracking)
            {
                return _db.User.Find(userId);
            }
            else
            {
                return _db.User.AsNoTracking().FirstOrDefault(item => item.UserId == userId);
            }
        }

        public User GetUser(string username)
        {
            return _db.User.Where(item => item.Username == username).FirstOrDefault();
        }

        public void DeleteUser(int userId)
        {
            User user = _db.User.Find(userId);
            _db.User.Remove(user);
            _db.SaveChanges();
        }
    }
}
