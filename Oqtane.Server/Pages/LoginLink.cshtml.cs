using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Managers;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class LoginLinkModel : PageModel
    {
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly IUserManager _userManager;
        private readonly ILogManager _logger;

        public LoginLinkModel(UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, IUserManager userManager, ILogManager logger)
        {
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string name, string token)
        {
            var returnurl = "/login";

            if (bool.Parse(HttpContext.GetSiteSettings().GetValue("LoginOptions:LoginLink", "false")) &&
                !User.Identity.IsAuthenticated && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(token))
            {
                var validuser = false;

                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(name);
                if (identityuser != null)
                {
                    var user = _userManager.GetUser(identityuser.UserName, HttpContext.GetAlias().SiteId);
                    if (user != null && user.TwoFactorCode == token && DateTime.UtcNow < user.TwoFactorExpiry)
                    {
                        await _identitySignInManager.SignInAsync(identityuser, false);
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Login Link Successful For User {Username}", name);
                        validuser = true;
                        returnurl = "/";
                    }
                }

                if (!validuser)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Login Link Failed For User {Username}", name);
                    returnurl += $"?status={ExternalLoginStatus.LoginLinkFailed}";
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Login Link Attempt For User {Username}", name);
                returnurl = "/";
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
