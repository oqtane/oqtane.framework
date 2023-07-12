using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _users;
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly ITenantManager _tenantManager;
        private readonly INotificationRepository _notifications;
        private readonly IFolderRepository _folders;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public UserManager(IUserRepository users, UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, ITenantManager tenantManager, INotificationRepository notifications, IFolderRepository folders, ISyncManager syncManager, ILogManager logger)
        {
            _users = users;
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _tenantManager = tenantManager;
            _notifications = notifications;
            _folders = folders;
            _syncManager = syncManager;
            _logger = logger;
        }

        public async Task<User> AddUser(User user)
        {
            User User = null;
            var alias = _tenantManager.GetAlias();
            bool succeeded = false;
            string errors = "";

            IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
            if (identityuser == null)
            {
                identityuser = new IdentityUser();
                identityuser.UserName = user.Username;
                identityuser.Email = user.Email;
                identityuser.EmailConfirmed = user.EmailConfirmed;
                var result = await _identityUserManager.CreateAsync(identityuser, user.Password);
                succeeded = result.Succeeded;
                if (!succeeded)
                {
                    errors = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                succeeded = result.Succeeded;
                if (!succeeded)
                {
                    errors = "Password Not Valid For User";
                }
                user.EmailConfirmed = succeeded;
            }

            if (succeeded)
            {
                user.LastLoginOn = null;
                user.LastIPAddress = "";
                User = _users.AddUser(user);
                _syncManager.AddSyncEvent(alias.TenantId, EntityNames.User, User.UserId, SyncEventActions.Create);
            }
            else
            {
                _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "Unable To Add User {Username} - {Errors}", user.Username, errors);
            }

            if (User != null)
            {
                if (!user.EmailConfirmed)
                {
                    string token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(identityuser);
                    string url = alias.Protocol + "://" + alias.Name + "/login?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                    string body = "Dear " + user.DisplayName + ",\n\nIn Order To Complete The Registration Of Your User Account Please Click The Link Displayed Below:\n\n" + url + "\n\nThank You!";
                    var notification = new Notification(user.SiteId, User, "User Account Verification", body);
                    _notifications.AddNotification(notification);
                }
                else
                {
                    string url = alias.Protocol + "://" + alias.Name;
                    string body = "Dear " + user.DisplayName + ",\n\nA User Account Has Been Successfully Created For You. Please Use The Following Link To Access The Site:\n\n" + url + "\n\nThank You!";
                    var notification = new Notification(user.SiteId, User, "User Account Notification", body);
                    _notifications.AddNotification(notification);
                }

                User.Password = ""; // remove sensitive information
                _logger.Log(user.SiteId, LogLevel.Information, this, LogFunction.Create, "User Added {User}", User);
            }
            else
            {
                user.Password = ""; // remove sensitive information
                _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "Unable To Add User {User}", user);
            }

            return User;
        }
    }
}
