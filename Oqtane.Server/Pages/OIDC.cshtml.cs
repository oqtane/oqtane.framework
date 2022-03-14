using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Oqtane.Pages
{
    public class OIDCModel : PageModel
    {
        public IActionResult OnGetAsync(string returnurl)
        {
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = !string.IsNullOrEmpty(returnurl) ? returnurl : "/" });
        }
    }
}
