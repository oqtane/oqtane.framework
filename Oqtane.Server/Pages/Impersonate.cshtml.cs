using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Managers;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    public class ImpersonateModel : PageModel
    {
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly IUserManager _userManager;
        private readonly ILogManager _logger;

        public ImpersonateModel(UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, IUserManager userManager, ILogManager logger)
        {
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(string username, string returnurl)
        {
            if (User.IsInRole(RoleNames.Admin) && !string.IsNullOrEmpty(username))
            {
                bool validuser = false;
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(username);
                if (identityuser != null)
                {
                    var alias = HttpContext.GetAlias();
                    var user = _userManager.GetUser(identityuser.UserName, alias.SiteId);
                    if (user != null && !user.IsDeleted && UserSecurity.ContainsRole(user.Roles, RoleNames.Registered) && !UserSecurity.ContainsRole(user.Roles, RoleNames.Host))
                    {
                        validuser = true;
                    }
                }

                if (validuser)
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "User {Username} Successfully Impersonated By Administrator {Administrator}", username, User.Identity.Name);

                    // note that .NET Identity uses a hardcoded ApplicationScheme of "Identity.Application" in SignInAsync
                    await _identitySignInManager.SignInAsync(identityuser, false);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Impersonation By Administrator {Administrator} Failed For User {Username} ", User.Identity.Name, username);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Attempt To Impersonate User {Username} By User {User}", username, User.Identity.Name);
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
