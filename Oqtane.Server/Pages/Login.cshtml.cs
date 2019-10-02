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

        private readonly UserManager<IdentityUser> IdentityUserManager;
        private readonly SignInManager<IdentityUser> IdentitySignInManager;

        public LoginModel(UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager)
        {
            this.IdentityUserManager = IdentityUserManager;
            this.IdentitySignInManager = IdentitySignInManager;
        }

        public async Task<IActionResult> OnPostAsync(string username, string password, bool remember, string returnurl)
        {
            bool validuser = false;
            IdentityUser identityuser = await IdentityUserManager.FindByNameAsync(username);
            if (identityuser != null)
            {
                var result = await IdentitySignInManager.CheckPasswordSignInAsync(identityuser, password, false);
                if (result.Succeeded)
                {
                    validuser = true;
                }
            }

            if (validuser)
            {
                await IdentitySignInManager.SignInAsync(identityuser, remember);
            }

            if (!returnurl.StartsWith("/"))
            {
                returnurl = "/" + returnurl;
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}