using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Managers;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class LogoutModel : PageModel
    {
        private readonly IUserManager _userManager;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public LogoutModel(IUserManager userManager, ISyncManager syncManager, ILogManager logger)
        {
            _userManager = userManager;
            _syncManager = syncManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(string returnurl, string everywhere)
        {
            if (HttpContext.User != null)
            {
                var alias = HttpContext.GetAlias();
                var user = _userManager.GetUser(HttpContext.User.Identity.Name, alias.SiteId);
                if (user != null)
                {
                    if (everywhere == "true")
                    {
                        await _userManager.LogoutUserEverywhere(user);
                    }
                    _syncManager.AddSyncEvent(alias, EntityNames.User, user.UserId, "Logout");
                    _syncManager.AddSyncEvent(alias, EntityNames.User, user.UserId, SyncEventActions.Reload);
                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout For Username {Username}", user.Username);
                }

                await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
            }

            returnurl = (returnurl == null) ? "/" : returnurl;
            returnurl = (!returnurl.StartsWith("/")) ? "/" + returnurl : returnurl;

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
