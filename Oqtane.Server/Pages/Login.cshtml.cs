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
            bool validuser = false;
            IdentityUser identityuser = await _identityUserManager.FindByNameAsync(username);
            if (identityuser != null)
            {
                var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, password, false);
                if (result.Succeeded)
                {
                    validuser = true;
                }
            }

            if (validuser)
            {
                await _identitySignInManager.SignInAsync(identityuser, remember);
            }

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