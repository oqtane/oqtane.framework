using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Oqtane.Repository;
using System.IO;
using System.Collections.Generic;
using Oqtane.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteAuthenticationBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthentication<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            builder.WithSiteAuthenticationOptions();

            return builder;
        }

        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthenticationOptions<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            // site OpenIdConnect options
            builder.AddSiteOptions<OpenIdConnectOptions>((options, alias) =>
            {
                if (alias.SiteSettings.GetValue("ExternalLogin:ProviderType", "") == "oidc")
                {
                    // default options
                    options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                    options.RequireHttpsMetadata = true;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-oidc" : "/" + alias.Path + "/signin-oidc";
                    options.ResponseType = OpenIdConnectResponseType.Code; // authorization code flow
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost; // recommended as most secure

                    // cookie config is required to avoid Correlation Failed errors
                    options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                    // site options
                    options.Authority = alias.SiteSettings.GetValue("ExternalLogin:Authority", "");
                    options.MetadataAddress = alias.SiteSettings.GetValue("ExternalLogin:MetadataUrl", "");
                    options.ClientId = alias.SiteSettings.GetValue("ExternalLogin:ClientId", "");
                    options.ClientSecret = alias.SiteSettings.GetValue("ExternalLogin:ClientSecret", "");
                    options.UsePkce = bool.Parse(alias.SiteSettings.GetValue("ExternalLogin:PKCE", "false"));
                    options.Scope.Clear();
                    foreach (var scope in alias.SiteSettings.GetValue("ExternalLogin:Scopes", "openid,profile,email").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        options.Scope.Add(scope);
                    }

                    // openid connect events
                    options.Events.OnTokenValidated = OnTokenValidated;
                    options.Events.OnAccessDenied = OnAccessDenied;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                }
            });

            // site OAuth2.0 options
            builder.AddSiteOptions<OAuthOptions>((options, alias) =>
            {
                if (alias.SiteSettings.GetValue("ExternalLogin:ProviderType", "") == "oauth2")
                {
                    // default options
                    options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                    options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-oauth2" : "/" + alias.Path + "/signin-oauth2";
                    options.SaveTokens = true;

                    // site options
                    options.AuthorizationEndpoint = alias.SiteSettings.GetValue("ExternalLogin:AuthorizationUrl", "");
                    options.TokenEndpoint = alias.SiteSettings.GetValue("ExternalLogin:TokenUrl", "");
                    options.UserInformationEndpoint = alias.SiteSettings.GetValue("ExternalLogin:UserInfoUrl", "");
                    options.ClientId = alias.SiteSettings.GetValue("ExternalLogin:ClientId", "");
                    options.ClientSecret = alias.SiteSettings.GetValue("ExternalLogin:ClientSecret", "");
                    options.UsePkce = bool.Parse(alias.SiteSettings.GetValue("ExternalLogin:PKCE", "false"));
                    options.Scope.Clear();
                    foreach (var scope in alias.SiteSettings.GetValue("ExternalLogin:Scopes", "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        options.Scope.Add(scope);
                    }

                    // cookie config is required to avoid Correlation Failed errors
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                    // oauth2 events
                    options.Events.OnCreatingTicket = OnCreatingTicket;
                    options.Events.OnAccessDenied = OnAccessDenied;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                }
            });

            return builder;
        }

        private static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
        {
            // OAuth 2.0
            var email = "";
            if (context.Options.UserInformationEndpoint != "")
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    var output = await response.Content.ReadAsStringAsync();

                    // get email address using Regex on the raw output (could be json or html)
                    var regex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
                    foreach (Match match in regex.Matches(output))
                    {
                        if (EmailValid(match.Value, context.HttpContext.GetAlias().SiteSettings.GetValue("ExternalLogin:DomainFilter", "")))
                        {
                            email = match.Value.ToLower();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "An Error Occurred Accessing The User Info Endpoint - {Error}", ex.Message);
                }
            }

            // login user
            await LoginUser(email, context.HttpContext, context.Principal);
        }

        private static async Task OnTokenValidated(TokenValidatedContext context)
        {
            // OpenID Connect
            var emailClaimType = context.HttpContext.GetAlias().SiteSettings.GetValue("ExternalLogin:EmailClaimType", "");
            var email = context.Principal.FindFirstValue(emailClaimType);

            // login user
            await LoginUser(email, context.HttpContext, context.Principal);
        }

        private static Task OnAccessDenied(AccessDeniedContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "External Login Access Denied - User May Have Cancelled Their External Login Attempt");
            // redirect to login page
            var alias = context.HttpContext.GetAlias();
            context.Response.Redirect(alias.Path + "/login?returnurl=" + context.Properties.RedirectUri, true);
            context.HandleResponse();
            return Task.CompletedTask;
        }

        private static Task OnRemoteFailure(RemoteFailureContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "External Login Remote Failure - {Error}", context.Failure.Message);
            // redirect to login page
            var alias = context.HttpContext.GetAlias();
            context.Response.Redirect(alias.Path + "/login", true);
            context.HandleResponse();
            return Task.CompletedTask;
        }

        private static async Task LoginUser(string email, HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            var _logger = httpContext.RequestServices.GetRequiredService<ILogManager>();
            var alias = httpContext.GetAlias();

            if (EmailValid(email, alias.SiteSettings.GetValue("ExternalLogin:DomainFilter", "")))
            {
                var _identityUserManager = httpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var _users = httpContext.RequestServices.GetRequiredService<IUserRepository>();
                var _userRoles = httpContext.RequestServices.GetRequiredService<IUserRoleRepository>();
                var providerType = httpContext.GetAlias().SiteSettings.GetValue("ExternalLogin:ProviderType", "");
                var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (providerKey == null)
                {
                    providerKey = email; // OAuth2 does not pass claims
                }
                User user = null;

                var identityuser = await _identityUserManager.FindByEmailAsync(email);
                if (identityuser == null)
                {
                    if (bool.Parse(alias.SiteSettings.GetValue("ExternalLogin:CreateUsers", "true")))
                    {
                        identityuser = new IdentityUser();
                        identityuser.UserName = email;
                        identityuser.Email = email;
                        identityuser.EmailConfirmed = true;
                        var result = await _identityUserManager.CreateAsync(identityuser, DateTime.UtcNow.ToString("yyyy-MMM-dd-HH-mm-ss"));
                        if (result.Succeeded)
                        {
                            // add user login
                            await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType, providerKey, ""));

                            user = new User();
                            user.SiteId = alias.SiteId;
                            user.Username = email;
                            user.DisplayName = email;
                            user.Email = email;
                            user.LastLoginOn = null;
                            user.LastIPAddress = "";
                            user = _users.AddUser(user);

                            // add folder for user
                            var _folders = httpContext.RequestServices.GetRequiredService<IFolderRepository>();
                            Folder folder = _folders.GetFolder(user.SiteId, Utilities.PathCombine("Users", Path.DirectorySeparatorChar.ToString()));
                            if (folder != null)
                            {
                                _folders.AddFolder(new Folder
                                {
                                    SiteId = folder.SiteId,
                                    ParentId = folder.FolderId,
                                    Name = "My Folder",
                                    Type = FolderTypes.Private,
                                    Path = Utilities.PathCombine(folder.Path, user.UserId.ToString(), Path.DirectorySeparatorChar.ToString()),
                                    Order = 1,
                                    ImageSizes = "",
                                    Capacity = Constants.UserFolderCapacity,
                                    IsSystem = true,
                                    Permissions = new List<Permission>
                                    {
                                        new Permission(PermissionNames.Browse, user.UserId, true),
                                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                        new Permission(PermissionNames.Edit, user.UserId, true)
                                    }.EncodePermissions()
                                });
                            }

                            // add auto assigned roles to user for site
                            var _roles = httpContext.RequestServices.GetRequiredService<IRoleRepository>();
                            List<Role> roles = _roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned).ToList();
                            foreach (Role role in roles)
                            {
                                UserRole userrole = new UserRole();
                                userrole.UserId = user.UserId;
                                userrole.RoleId = role.RoleId;
                                userrole.EffectiveDate = null;
                                userrole.ExpiryDate = null;
                                _userRoles.AddUserRole(userrole);
                            }
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Creation Of New Users Is Disabled. User With Email Address {Email} Will First Need To Be Registered On The Site.", email);
                    }
                }
                else
                {
                    var logins = await _identityUserManager.GetLoginsAsync(identityuser);
                    var login = logins.FirstOrDefault(item => item.LoginProvider == providerType);
                    if (login != null)
                    {
                        if (login.ProviderKey == providerKey)
                        {
                            user = _users.GetUser(identityuser.UserName);
                        }
                        else
                        {
                            // provider keys do not match
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Key Does Not Match For User {Username}. Login Denied.", identityuser.UserName);
                        }
                    }
                    else
                    {
                        // add user login
                        await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType, providerKey, ""));
                        user = _users.GetUser(identityuser.UserName);
                    }
                }

                // add claims to principal
                if (user != null)
                {
                    // update user
                    user.LastLoginOn = DateTime.UtcNow;
                    user.LastIPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                    _users.UpdateUser(user);
                    _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "User Login Successful For {Username} Using Provider {Provider}", user.Username, providerType);

                    // add Oqtane claims
                    var principal = (ClaimsIdentity)claimsPrincipal.Identity;
                    UserSecurity.ResetClaimsIdentity(principal);
                    List<UserRole> userroles = _userRoles.GetUserRoles(user.UserId, user.SiteId).ToList();
                    var identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                    principal.AddClaims(identity.Claims);
                }
                else // user not logged in
                {
                    await httpContext.SignOutAsync();
                }
            }
            else // email invalid
            {
                if (!string.IsNullOrEmpty(email))
                {
                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The Email Address {Email} Is Invalid Or Does Not Match The Domain Filter Criteria. Login Denied.", email);
                }
                else
                {
                    var emailclaimtype = claimsPrincipal.Claims.FirstOrDefault(item => item.Value.Contains("@") && item.Value.Contains("."));
                    if (emailclaimtype != null)
                    {
                        _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Please Verify If \"{ClaimType}\" Is A Valid Email Claim Type For The Provider And Update Your External Login Settings Accordingly", emailclaimtype.Type);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Did Not Return An Email To Uniquely Identify The User.");
                    }
                }
                await httpContext.SignOutAsync();
            }
        }

        private static bool EmailValid(string email, string domainfilter)
        {
            if (!string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains("."))
            {
                var domains = domainfilter.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var domain in domains)
                {
                    if (domain.StartsWith("!"))
                    {
                        if (email.ToLower().Contains(domain.Substring(1))) return false;
                    }
                    else
                    {
                        if (!email.ToLower().Contains(domain)) return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
