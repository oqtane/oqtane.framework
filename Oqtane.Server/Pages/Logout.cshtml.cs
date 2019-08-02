using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Models;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync(string returnurl)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            string url = "~/";
            if (returnurl != "/")
            {
                url = Url.Content("~/" + returnurl);
            }
            return LocalRedirect(url);
        }
    }
}