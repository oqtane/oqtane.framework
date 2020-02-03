using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Models;
using Oqtane.Repository;
using System.Threading.Tasks;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class VerifyModel : PageModel
    {
        private readonly IUserRepository Users;
        private readonly UserManager<IdentityUser> IdentityUserManager;

        public VerifyModel(IUserRepository Users, UserManager<IdentityUser> IdentityUserManager)
        {
            this.Users = Users;
            this.IdentityUserManager = IdentityUserManager;
        }

        public async Task<IActionResult> OnGet(string name, string token, string returnurl)
        {
            int verified = 0;
            IdentityUser identityuser = await IdentityUserManager.FindByNameAsync(name);
            if (identityuser != null)
            {
                var result = await IdentityUserManager.ConfirmEmailAsync(identityuser, token);
                if (result.Succeeded)
                {
                    verified = 1;
                }
            }
            if (!returnurl.StartsWith("/"))
            {
                returnurl += "/" + returnurl;
            }
            return Redirect(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + returnurl + "login?verified=" + verified.ToString());
        }
    }
}