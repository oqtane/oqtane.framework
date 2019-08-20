using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {

        private readonly UserManager<IdentityUser> identityUserManager;
        private readonly SignInManager<IdentityUser> identitySignInManager;

        public LoginModel(UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager)
        {
            identityUserManager = IdentityUserManager;
            identitySignInManager = IdentitySignInManager;
        }

        public async Task<IActionResult> OnPostAsync(string username, string password, bool remember, string returnurl)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            bool validuser = false;
            IdentityUser identityuser = await identityUserManager.FindByNameAsync(username);
            if (identityuser != null)
            {
                var result = await identitySignInManager.CheckPasswordSignInAsync(identityuser, password, false);
                if (result.Succeeded)
                {
                    validuser = true;
                }
            }

            if (validuser)
            {
                var claims = new List<Claim>{ new Claim(ClaimTypes.Name, username) };
                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var authProperties = new AuthenticationProperties{IsPersistent = remember};
                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}