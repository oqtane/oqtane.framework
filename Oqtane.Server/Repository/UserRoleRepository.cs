using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface IUserRoleRepository
    {
        IEnumerable<UserRole> GetUserRoles(int siteId);
        IEnumerable<UserRole> GetUserRoles(int userId, int siteId);
        IEnumerable<UserRole> GetUserRoles(string roleName, int siteId);
        UserRole AddUserRole(UserRole userRole);
        UserRole UpdateUserRole(UserRole userRole);
        UserRole GetUserRole(int userRoleId);
        UserRole GetUserRole(int userRoleId, bool tracking);
        UserRole GetUserRole(int userId, int roleId);
        UserRole GetUserRole(int userId, int roleId, bool tracking);
        void DeleteUserRole(int userRoleId);
        void DeleteUserRoles(int userId);
    }

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IRoleRepository _roles;
        private readonly ITenantManager _tenantManager;
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly IMemoryCache _cache;

        public UserRoleRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IRoleRepository roles, ITenantManager tenantManager, UserManager<IdentityUser> identityUserManager, IMemoryCache cache)
        {
            _dbContextFactory = dbContextFactory;
            _roles = roles;
            _tenantManager = tenantManager;
            _identityUserManager = identityUserManager;
            _cache = cache;
        }

        public IEnumerable<UserRole> GetUserRoles(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => item.Role.SiteId == siteId || item.Role.SiteId == null || siteId == -1).ToList();
        }

        public IEnumerable<UserRole> GetUserRoles(int userId, int siteId)
        {
            var alias = _tenantManager.GetAlias();
            return _cache.GetOrCreate($"userroles:{userId}:{alias.SiteKey}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                using var db = _dbContextFactory.CreateDbContext();
                return db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .Where(item => (item.Role.SiteId == siteId || item.Role.SiteId == null || siteId == -1) && item.UserId == userId).ToList();
            });
        }

        public IEnumerable<UserRole> GetUserRoles(string roleName, int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.UserRole
                .Include(item => item.Role) // eager load roles
                .Include(item => item.User) // eager load users
                .Where(item => (item.Role.SiteId == siteId || item.Role.SiteId == null || siteId == -1) && item.Role.Name == roleName).ToList();
        }

        public UserRole AddUserRole(UserRole userRole)
        {
            userRole.EffectiveDate = userRole.EffectiveDate.HasValue ? DateTime.SpecifyKind(userRole.EffectiveDate.Value, DateTimeKind.Utc) : userRole.EffectiveDate;
            userRole.ExpiryDate = userRole.ExpiryDate.HasValue ? DateTime.SpecifyKind(userRole.ExpiryDate.Value, DateTimeKind.Utc) : userRole.ExpiryDate;

            using var db = _dbContextFactory.CreateDbContext();
            db.UserRole.Add(userRole);
            db.SaveChanges();

            // host roles can only exist at global level - remove any site specific user roles
            var role = _roles.GetRole(userRole.RoleId);
            if (role.Name == RoleNames.Host)
            {
                DeleteUserRoles(userRole.UserId);
            }

            if (!userRole.IgnoreSecurityStamp)
            {
                UpdateSecurityStamp(userRole.UserId);
            }

            RefreshCache(userRole.UserId);

            return userRole;
        }

        public UserRole UpdateUserRole(UserRole userRole)
        {
            userRole.EffectiveDate = userRole.EffectiveDate.HasValue ? DateTime.SpecifyKind(userRole.EffectiveDate.Value, DateTimeKind.Utc) : userRole.EffectiveDate;
            userRole.ExpiryDate = userRole.ExpiryDate.HasValue ? DateTime.SpecifyKind(userRole.ExpiryDate.Value, DateTimeKind.Utc) : userRole.ExpiryDate;

            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(userRole).State = EntityState.Modified;
            db.SaveChanges();

            if (!userRole.IgnoreSecurityStamp)
            {
                UpdateSecurityStamp(userRole.UserId);
            }

            RefreshCache(userRole.UserId);

            return userRole;
        }

        public UserRole GetUserRole(int userRoleId)
        {
            return GetUserRole(userRoleId, true);
        }

        public UserRole GetUserRole(int userRoleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
            else
            {
                return db.UserRole.AsNoTracking()
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserRoleId == userRoleId);
            }
        }

        public UserRole GetUserRole(int userId, int roleId)
        {
            return GetUserRole(userId, roleId, true);
        }

        public UserRole GetUserRole(int userId, int roleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UserRole
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserId == userId && item.RoleId == roleId);
            }
            else
            {
                return db.UserRole.AsNoTracking()
                    .Include(item => item.Role) // eager load roles
                    .Include(item => item.User) // eager load users
                    .FirstOrDefault(item => item.UserId == userId && item.RoleId == roleId);
            }
        }

        public void DeleteUserRole(int userRoleId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var userRole = db.UserRole.Find(userRoleId);
            db.UserRole.Remove(userRole);
            db.SaveChanges();

            UpdateSecurityStamp(userRole.UserId);
            RefreshCache(userRole.UserId);
        }

        public void DeleteUserRoles(int userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            foreach (var userRole in db.UserRole.Where(item => item.UserId == userId && item.Role.SiteId != null))
            {
                db.UserRole.Remove(userRole);
            }
            db.SaveChanges();

            UpdateSecurityStamp(userId);
            RefreshCache(userId);
        }

        private void UpdateSecurityStamp(int userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var user = db.User.Find(userId);
            if (user != null)
            {
                var identityuser = _identityUserManager.FindByNameAsync(user.Username).GetAwaiter().GetResult();
                if (identityuser != null)
                {
                    _identityUserManager.UpdateSecurityStampAsync(identityuser).GetAwaiter().GetResult();
                }
            }
        }

        private void RefreshCache(int userId)
        {
            var alias = _tenantManager.GetAlias();
            if (alias != null)
            {
                _cache.Remove($"user:{userId}:{alias.SiteKey}");
                _cache.Remove($"userroles:{userId}:{alias.SiteKey}");
            }
        }
    }
}
