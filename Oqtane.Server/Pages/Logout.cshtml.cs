using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Extensions;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string returnurl)
        {
            await HttpContext.SignOutAsync(Constants.AuthenticationScheme);

            returnurl = (returnurl == null) ? "/" : returnurl;
            returnurl = (!returnurl.StartsWith("/")) ? "/" + returnurl : returnurl;

            var provider = HttpContext.User.Claims.FirstOrDefault(item => item.Type == "Provider");
            var authority = HttpContext.GetAlias().SiteSettings.GetValue("OpenIdConnectOptions:Authority", "");
            var logoutUrl = HttpContext.GetAlias().SiteSettings.GetValue("OpenIdConnectOptions:LogoutUrl", "");
            if (provider != null && provider.Value == authority && logoutUrl != "")
            {
                return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties { RedirectUri = returnurl });
            }
            else
            {
                return LocalRedirect(Url.Content("~" + returnurl));
            }
        }
    }
}
