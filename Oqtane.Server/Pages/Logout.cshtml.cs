using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync(string returnurl)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            if (returnurl == null)
            {
                returnurl = "";
            }
            if (!returnurl.StartsWith("/"))
            {
                returnurl = "/" + returnurl;
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}