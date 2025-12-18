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
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class PasskeyModel : PageModel
    {
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly IUserManager _userManager;
        private readonly ILogManager _logger;

        public PasskeyModel(UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, IUserManager userManager, ILogManager logger)
        {
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(string operation, string credential, string returnurl)
        {
            if (HttpContext.GetSiteSettings().GetValue("LoginOptions:Passkeys", "false") == "true")
            {
                IdentityUser identityuser;

                switch (operation.ToLower())
                {
                    case "create":
                        if (User.Identity.IsAuthenticated)
                        {
                            identityuser = await _identityUserManager.FindByNameAsync(User.Identity.Name);
                            if (identityuser != null)
                            {
                                var creationOptionsJson = await _identitySignInManager.MakePasskeyCreationOptionsAsync(new()
                                {
                                    Id = identityuser.Id,
                                    Name = identityuser.UserName,
                                    DisplayName = identityuser.UserName
                                });
                                returnurl += (!returnurl.Contains("?") ? "?" : "&") + $"options={WebUtility.UrlEncode(creationOptionsJson)}";
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Create Attempt - User {User} Does Not Exist", User.Identity.Name);
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Create Attempt - User Not Authenticated");
                        }
                        break;
                    case "validate":
                        if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(credential))
                        {
                            identityuser = await _identityUserManager.FindByNameAsync(User.Identity.Name);
                            if (identityuser != null)
                            {
                                var attestationResult = await _identitySignInManager.PerformPasskeyAttestationAsync(credential);
                                if (attestationResult.Succeeded)
                                {
                                    var user = _userManager.GetUser(User.Identity.Name, HttpContext.GetAlias().SiteId);
                                    if (user != null && !user.IsDeleted && UserSecurity.ContainsRole(user.Roles, RoleNames.Registered))
                                    {
                                        // setting a default name and including a SiteId prefix for multi-tenancy
                                        var name = (!string.IsNullOrEmpty(user.DisplayName)) ? user.DisplayName : user.Username;
                                        attestationResult.Passkey.Name = HttpContext.GetAlias().SiteId + ":" + name + "'s Passkey";
                                        var addPasskeyResult = await _identityUserManager.AddOrUpdatePasskeyAsync(identityuser, attestationResult.Passkey);
                                    }
                                    else
                                    {
                                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Passkey Validation Failed - User {Username} Is Deleted Or Is Not A Registered User For The Site", User.Identity.Name);
                                    }
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Passkey Validation Failed For User {Username} - {Message}", User.Identity.Name, attestationResult.Failure.Message);
                                }
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Validation Attempt - User {User} Does Not Exist", User.Identity.Name);
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Validation Attempt - User Not Authenticated Or Credential Not Provided");
                        }
                        break;
                    case "request":
                        if (!User.Identity.IsAuthenticated)
                        {
                            identityuser = null;
                            var requestOptionsJson = await _identitySignInManager.MakePasskeyRequestOptionsAsync(identityuser);
                            returnurl = HttpContext.GetAlias().Path + $"/login?options={WebUtility.UrlEncode(requestOptionsJson)}&returnurl={WebUtility.UrlEncode(returnurl)}";
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Request Attempt - User Is Already Authenticated");
                        }
                        break;
                    case "login":
                        if (!User.Identity.IsAuthenticated && !string.IsNullOrEmpty(credential))
                        {
                            var result = await _identitySignInManager.PasskeySignInAsync(credential);
                            if (result.Succeeded)
                            {
                                var user = _userManager.GetUser(User.Identity.Name, HttpContext.GetAlias().SiteId);
                                if (user != null && !user.IsDeleted && UserSecurity.ContainsRole(user.Roles, RoleNames.Registered))
                                {
                                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "Passkey Login Successful For User {Username}", User.Identity.Name);
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "Passkey Login Failed - User {Username} Is Deleted Or Is Not A Registered User For The Site", User.Identity.Name);
                                }
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Passkey Login Failed - Invalid Credential");
                                returnurl = HttpContext.GetAlias().Path + $"/login?status={ExternalLoginStatus.PasskeyFailed}&returnurl={WebUtility.UrlEncode(returnurl)}";
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Login Attempt");
                        }
                        break;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Passkey Request - Passkeys Are Not Enabled For Site");
            }

            if (!returnurl.StartsWith("/"))
            {
                returnurl = "/" + returnurl;
            }

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
