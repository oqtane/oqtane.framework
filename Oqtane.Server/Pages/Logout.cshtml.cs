using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public LogoutModel(IUserManager userManager, ISyncManager syncManager)
        {
            _userManager = userManager;
            _syncManager = syncManager;
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
                }

                await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
            }

            returnurl = (returnurl == null) ? "/" : returnurl;
            returnurl = (!returnurl.StartsWith("/")) ? "/" + returnurl : returnurl;

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
