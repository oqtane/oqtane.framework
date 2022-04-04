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
using Oqtane.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteAuthenticationBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder WithSiteAuthentication(this OqtaneSiteOptionsBuilder builder)
        {
            // site cookie authentication options
            builder.AddSiteOptions<CookieAuthenticationOptions>((options, alias, sitesettings) =>
            {
                if (sitesettings.GetValue("LoginOptions:CookieType", "domain") == "domain")
                {
                    options.Cookie.Name = ".AspNetCore.Identity.Application";
                }
                else
                {
                    // use unique cookie name for site
                    options.Cookie.Name = ".AspNetCore.Identity.Application" + alias.SiteKey;
                }
            });

            // site OpenId Connect options
            builder.AddSiteOptions<OpenIdConnectOptions>((options, alias, sitesettings) =>
            {
                if (sitesettings.GetValue("ExternalLogin:ProviderType", "") == AuthenticationProviderTypes.OpenIDConnect)
                {
                    // default options
                    options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                    options.RequireHttpsMetadata = true;
                    options.SaveTokens = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-" + AuthenticationProviderTypes.OpenIDConnect : "/" + alias.Path + "/signin-" + AuthenticationProviderTypes.OpenIDConnect;
                    options.ResponseType = OpenIdConnectResponseType.Code; // authorization code flow
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost; // recommended as most secure

                    // cookie config is required to avoid Correlation Failed errors
                    options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                    // site options
                    options.Authority = sitesettings.GetValue("ExternalLogin:Authority", "");
                    options.MetadataAddress = sitesettings.GetValue("ExternalLogin:MetadataUrl", "");
                    options.ClientId = sitesettings.GetValue("ExternalLogin:ClientId", "");
                    options.ClientSecret = sitesettings.GetValue("ExternalLogin:ClientSecret", "");
                    options.UsePkce = bool.Parse(sitesettings.GetValue("ExternalLogin:PKCE", "false"));
                    options.Scope.Clear();
                    foreach (var scope in sitesettings.GetValue("ExternalLogin:Scopes", "openid,profile,email").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        options.Scope.Add(scope);
                    }

                    // openid connect events
                    options.Events.OnTokenValidated = OnTokenValidated;
                    options.Events.OnAccessDenied = OnAccessDenied;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                }
            });

            // site OAuth 2.0 options
            builder.AddSiteOptions<OAuthOptions>((options, alias, sitesettings) =>
            {
                if (sitesettings.GetValue("ExternalLogin:ProviderType", "") == AuthenticationProviderTypes.OAuth2)
                {
                    // default options
                    options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                    options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-" + AuthenticationProviderTypes.OAuth2 : "/" + alias.Path + "/signin-" + AuthenticationProviderTypes.OAuth2;
                    options.SaveTokens = false;

                    // site options
                    options.AuthorizationEndpoint = sitesettings.GetValue("ExternalLogin:AuthorizationUrl", "");
                    options.TokenEndpoint = sitesettings.GetValue("ExternalLogin:TokenUrl", "");
                    options.UserInformationEndpoint = sitesettings.GetValue("ExternalLogin:UserInfoUrl", "");
                    options.ClientId = sitesettings.GetValue("ExternalLogin:ClientId", "");
                    options.ClientSecret = sitesettings.GetValue("ExternalLogin:ClientSecret", "");
                    options.UsePkce = bool.Parse(sitesettings.GetValue("ExternalLogin:PKCE", "false"));
                    options.Scope.Clear();
                    foreach (var scope in sitesettings.GetValue("ExternalLogin:Scopes", "").Split(',', StringSplitOptions.RemoveEmptyEntries))
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
                        if (EmailValid(match.Value, context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
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
            var emailClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", "");
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

            if (EmailValid(email, httpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
            {
                var _identityUserManager = httpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var _users = httpContext.RequestServices.GetRequiredService<IUserRepository>();
                var _userRoles = httpContext.RequestServices.GetRequiredService<IUserRoleRepository>();
                var providerType = httpContext.GetSiteSettings().GetValue("ExternalLogin:ProviderType", "");
                var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (providerKey == null)
                {
                    providerKey = email; // OAuth2 does not pass claims
                }
                User user = null;

                bool duplicates = false;
                IdentityUser identityuser = null;
                try
                {
                    identityuser = await _identityUserManager.FindByEmailAsync(email);
                }
                catch
                {
                    // FindByEmailAsync will throw an error if the email matches multiple user accounts
                    duplicates = true;
                }
                if (identityuser == null)
                {
                    if (duplicates)
                    {
                        _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Multiple Users Exist With Email Address {Email}. Login Denied.", email);
                    }
                    else
                    {
                        if (bool.Parse(httpContext.GetSiteSettings().GetValue("ExternalLogin:CreateUsers", "true")))
                        {
                            identityuser = new IdentityUser();
                            identityuser.UserName = email;
                            identityuser.Email = email;
                            identityuser.EmailConfirmed = true;
                            var result = await _identityUserManager.CreateAsync(identityuser, DateTime.UtcNow.ToString("yyyy-MMM-dd-HH-mm-ss"));
                            if (result.Succeeded)
                            {
                                user = new User
                                {
                                    SiteId = httpContext.GetAlias().SiteId,
                                    Username = email,
                                    DisplayName = email,
                                    Email = email,
                                    LastLoginOn = null,
                                    LastIPAddress = ""
                                };
                                user = _users.AddUser(user);

                                if (user != null)
                                {
                                    var _notifications = httpContext.RequestServices.GetRequiredService<INotificationRepository>();
                                    string url = httpContext.Request.Scheme + "://" + httpContext.GetAlias().Name;
                                    string body = "You Recently Used An External Account To Sign In To Our Site.\n\n" + url + "\n\nThank You!";
                                    var notification = new Notification(user.SiteId, user, "User Account Notification", body);
                                    _notifications.AddNotification(notification);

                                    // add user login
                                    await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType, providerKey, ""));

                                    _logger.Log(user.SiteId, LogLevel.Information, "ExternalLogin", Enums.LogFunction.Create, "User Added {User}", user);
                                }
                                else
                                {
                                    _logger.Log(user.SiteId, LogLevel.Error, "ExternalLogin", Enums.LogFunction.Create, "Unable To Add User {Email}", email);
                                }
                            }
                            else
                            {
                                _logger.Log(user.SiteId, LogLevel.Error, "ExternalLogin", Enums.LogFunction.Create, "Unable To Add Identity User {Email} {Error}", email, result.Errors.ToString());
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Creation Of New Users Is Disabled For This Site. User With Email Address {Email} Will First Need To Be Registered On The Site.", email);
                        }
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
                        _logger.Log(user.SiteId, LogLevel.Information, "ExternalLogin", Enums.LogFunction.Create, "External User Login Added For {Email} Using Provider {Provider}", email, providerType);
                    }
                }

                // add claims to principal
                if (user != null)
                {
                    var principal = (ClaimsIdentity)claimsPrincipal.Identity;
                    UserSecurity.ResetClaimsIdentity(principal);
                    var identity = UserSecurity.CreateClaimsIdentity(httpContext.GetAlias(), user, _userRoles.GetUserRoles(user.UserId, user.SiteId).ToList());
                    principal.AddClaims(identity.Claims);

                    // update user
                    user.LastLoginOn = DateTime.UtcNow;
                    user.LastIPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                    _users.UpdateUser(user);
                    _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "External User Login Successful For {Username} Using Provider {Provider}", user.Username, providerType);
                }
                else // user not valid
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
