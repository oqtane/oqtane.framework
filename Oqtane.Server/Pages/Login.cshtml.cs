using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;

        public LoginModel(UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager)
        {
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
        }

         public async Task<IActionResult> OnPostAsync(string username, string password, bool remember, string returnurl)
        {
            if (!User.Identity.IsAuthenticated && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                bool validuser = false;
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(username);
                if (identityuser != null)
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, password, true);
                    if (result.Succeeded)
                    {
                        validuser = true;
                    }
                }

                if (validuser)
                {
                    await _identitySignInManager.SignInAsync(identityuser, remember);
                }
            }

            if (returnurl == null)
            {
                returnurl = "";
            }
            else
            {
                returnurl = WebUtility.UrlDecode(returnurl);
            }
            if (!returnurl.StartsWith("/"))
            {
                returnurl = "/" + returnurl;
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
