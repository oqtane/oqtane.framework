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
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Text.Json.Nodes;
using System.Globalization;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteAuthenticationBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder WithSiteAuthentication(this OqtaneSiteOptionsBuilder builder)
        {
            // site cookie authentication options
            builder.AddSiteOptions<CookieAuthenticationOptions>((options, alias, sitesettings) =>
            {
                options.Cookie.Name = sitesettings.GetValue("LoginOptions:CookieName", ".AspNetCore.Identity.Application");
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
                    if (!string.IsNullOrEmpty(sitesettings.GetValue("ExternalLogin:RoleClaimType", "")))
                    {
                        options.TokenValidationParameters.RoleClaimType = sitesettings.GetValue("ExternalLogin:RoleClaimType", "");
                    }
                    options.Scope.Clear();
                    foreach (var scope in sitesettings.GetValue("ExternalLogin:Scopes", "openid,profile,email").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        options.Scope.Add(scope);
                    }

                    // openid connect events
                    options.Events.OnTokenValidated = OnTokenValidated;
                    options.Events.OnAccessDenied = OnAccessDenied;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                    if (sitesettings.GetValue("ExternalLogin:Parameters", "") != "")
                    {
                        options.Events = new OpenIdConnectEvents
                        {
                            OnRedirectToIdentityProvider = context =>
                            {
                                foreach (var parameter in sitesettings.GetValue("ExternalLogin:Parameters", "").Split(","))
                                {
                                    context.ProtocolMessage.SetParameter(parameter.Split("=")[0], parameter.Split("=")[1]);
                                }
                                return Task.FromResult(0);
                            }
                        };
                    }
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
                    options.Events.OnTicketReceived = OnTicketReceived;
                    options.Events.OnAccessDenied = OnAccessDenied;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                    if (sitesettings.GetValue("ExternalLogin:Parameters", "") != "")
                    {
                        options.Events = new OAuthEvents
                        {
                            OnRedirectToAuthorizationEndpoint = context =>
                            {
                                var url = context.RedirectUri;
                                foreach (var parameter in sitesettings.GetValue("ExternalLogin:Parameters", "").Split(","))
                                {
                                    url += (!url.Contains("?")) ? "?" + parameter : "&" + parameter;
                                }
                                context.Response.Redirect(url);
                                return Task.FromResult(0);
                            }
                        };
                    }
                }
            });

            return builder;
        }

        private static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
        {
            // OAuth 2.0
            var email = "";
            var id = "";
            var claims = "";

            if (context.Options.UserInformationEndpoint != "")
            {
                try
                {
                    // call user information endpoint
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    claims = await response.Content.ReadAsStringAsync();

                    // parse json output
                    var idClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:IdentifierClaimType", "");
                    var emailClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", "");
                    if (!claims.StartsWith("[") && !claims.EndsWith("]"))
                    {
                        claims = "[" + claims + "]"; // convert to json array
                    }
                    JsonNode items = JsonNode.Parse(claims)!;
                    foreach (var item in items.AsArray())
                    {
                        if (item[emailClaimType] != null)
                        {
                            if (EmailValid(item[emailClaimType].ToString(), context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
                            {
                                email = item[emailClaimType].ToString().ToLower();
                                if (item[idClaimType] != null)
                                {
                                    id = item[idClaimType].ToString();
                                }
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(id))
                    {
                        id = email;
                    }
                }
                catch (Exception ex)
                {
                    var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, ex, "An Error Occurred Accessing The User Info Endpoint - {Error}", ex.Message);
                }
            }

            // validate user
            var identity = await ValidateUser(email, id, claims, context.HttpContext, context.Principal);
            if (identity.Label == ExternalLoginStatus.Success)
            {
                identity.AddClaim(new Claim("access_token", context.AccessToken));
                context.Principal = new ClaimsPrincipal(identity);
            }

            // pass properties to OnTicketReceived
            context.Properties.SetParameter("status", identity.Label);
            context.Properties.SetParameter("redirecturl", context.Properties.RedirectUri);
        }

        private static Task OnTicketReceived(TicketReceivedContext context)
        {
            // OAuth 2.0
            var status = context.Properties.GetParameter<string>("status");
            if (status != ExternalLoginStatus.Success)
            {
                // redirect to login page and pass status
                context.Response.Redirect(Utilities.TenantUrl(context.HttpContext.GetAlias(), $"/login?status={status}&returnurl={context.Properties.GetParameter<string>("redirecturl")}"), true);
                context.HandleResponse();
            }
            return Task.CompletedTask;
        }

        private static async Task OnTokenValidated(TokenValidatedContext context)
        {
            // OpenID Connect
            var idClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:IdentifierClaimType", "");
            var id = context.Principal.FindFirstValue(idClaimType);
            var emailClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", "");
            var email = context.Principal.FindFirstValue(emailClaimType);
            var claims = string.Join(", ", context.Principal.Claims.Select(item => item.Type).ToArray());

            // validate user
            var identity = await ValidateUser(email, id, claims, context.HttpContext, context.Principal);
            if (identity.Label == ExternalLoginStatus.Success)
            {
                // external roles
                if (!string.IsNullOrEmpty(context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:RoleClaimType", "")))
                {
                    foreach (var claim in context.Principal.Claims.Where(item => item.Type == ClaimTypes.Role))
                    {
                        if (!identity.Claims.Any(item => item.Type == ClaimTypes.Role && item.Value == claim.Value))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                        }
                    }
                }

                identity.AddClaim(new Claim("access_token", context.SecurityToken.RawData));
                context.Principal = new ClaimsPrincipal(identity);
            }
            else
            {
                // redirect to login page and pass status
                context.Response.Redirect(Utilities.TenantUrl(context.HttpContext.GetAlias(), $"/login?status={identity.Label}&returnurl={context.Properties.RedirectUri}"), true);
                context.HandleResponse();
            }
        }

        private static Task OnAccessDenied(AccessDeniedContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "External Login Access Denied - User May Have Cancelled Their External Login Attempt");
            // redirect to login page
            context.Response.Redirect(Utilities.TenantUrl(context.HttpContext.GetAlias(), $"/login?status={ExternalLoginStatus.AccessDenied}&returnurl={context.Properties.RedirectUri}"), true);
            context.HandleResponse();
            return Task.CompletedTask;
        }

        private static Task OnRemoteFailure(RemoteFailureContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "External Login Remote Failure - {Error}", context.Failure.Message);
            // redirect to login page
            context.Response.Redirect(Utilities.TenantUrl(context.HttpContext.GetAlias(), $"/login?status={ExternalLoginStatus.RemoteFailure}"), true);
            context.HandleResponse();
            return Task.CompletedTask;
        }

        private static async Task<ClaimsIdentity> ValidateUser(string email, string id, string claims, HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            var _logger = httpContext.RequestServices.GetRequiredService<ILogManager>();
            ClaimsIdentity identity = new ClaimsIdentity(Constants.AuthenticationScheme);
            // use identity.Label as a temporary location to store validation status information

            var providerType = httpContext.GetSiteSettings().GetValue("ExternalLogin:ProviderType", "");
            var providerName = httpContext.GetSiteSettings().GetValue("ExternalLogin:ProviderName", "");
            var alias = httpContext.GetAlias();
            var _users = httpContext.RequestServices.GetRequiredService<IUserRepository>();
            User user = null;

            if (!string.IsNullOrEmpty(id))
            {
                // verify if external user is already registered for this site
                var _identityUserManager = httpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var identityuser = await _identityUserManager.FindByLoginAsync(providerType + ":" + alias.SiteId.ToString(), id);
                if (identityuser != null)
                {
                    user = _users.GetUser(identityuser.UserName);
                }
                else
                {
                    if (EmailValid(email, httpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
                    {
                        bool duplicates = false;
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
                                identity.Label = ExternalLoginStatus.DuplicateEmail;
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
                                    var result = await _identityUserManager.CreateAsync(identityuser, DateTime.UtcNow.ToString("yyyy-MMM-dd-HH-mm-ss", CultureInfo.InvariantCulture));
                                    if (result.Succeeded)
                                    {
                                        user = new User
                                        {
                                            SiteId = alias.SiteId,
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
                                            string url = httpContext.Request.Scheme + "://" + alias.Name;
                                            string body = "You Recently Used An External Account To Sign In To Our Site.\n\n" + url + "\n\nThank You!";
                                            var notification = new Notification(user.SiteId, user, "User Account Notification", body);
                                            _notifications.AddNotification(notification);

                                            // add user login
                                            await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType + ":" + alias.SiteId.ToString(), id, providerName));

                                            _logger.Log(user.SiteId, LogLevel.Information, "ExternalLogin", Enums.LogFunction.Create, "User Added {User}", user);
                                        }
                                        else
                                        {
                                            identity.Label = ExternalLoginStatus.UserNotCreated;
                                            _logger.Log(alias.SiteId, LogLevel.Error, "ExternalLogin", Enums.LogFunction.Create, "Unable To Add User {Email}", email);
                                        }
                                    }
                                    else
                                    {
                                        identity.Label = ExternalLoginStatus.UserNotCreated;
                                        _logger.Log(alias.SiteId, LogLevel.Error, "ExternalLogin", Enums.LogFunction.Create, "Unable To Add Identity User {Email} {Error}", email, result.Errors.ToString());
                                    }
                                }
                                else
                                {
                                    identity.Label = ExternalLoginStatus.UserDoesNotExist;
                                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Creation Of New Users Is Disabled For This Site. User With Email Address {Email} Will First Need To Be Registered On The Site.", email);
                                }
                            }
                        }
                        else
                        {
                            var logins = await _identityUserManager.GetLoginsAsync(identityuser);
                            var login = logins.FirstOrDefault(item => item.LoginProvider == (providerType + ":" + alias.SiteId.ToString()));
                            if (login == null)
                            {
                                // new external login using existing user account - verification required
                                var _notifications = httpContext.RequestServices.GetRequiredService<INotificationRepository>();
                                string token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(identityuser);
                                string url = httpContext.Request.Scheme + "://" + alias.Name;
                                url += $"/login?name={identityuser.UserName}&token={WebUtility.UrlEncode(token)}&key={WebUtility.UrlEncode(id)}";
                                string body = $"You Recently Signed In To Our Site With {providerName} Using The Email Address {email}. ";
                                body += "In Order To Complete The Linkage Of Your User Account Please Click The Link Displayed Below:\n\n" + url + "\n\nThank You!";
                                var notification = new Notification(alias.SiteId, email, email, "External Login Linkage", body);
                                _notifications.AddNotification(notification);

                                identity.Label = ExternalLoginStatus.VerificationRequired;
                                _logger.Log(alias.SiteId, LogLevel.Information, "ExternalLogin", Enums.LogFunction.Create, "External Login Linkage Verification For Provider {Provider} Sent To {Email}", providerName, email);
                            }
                            else
                            {
                                // provider keys do not match
                                identity.Label = ExternalLoginStatus.ProviderKeyMismatch;
                                _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Key Does Not Match For User {Username}. Login Denied.", identityuser.UserName);
                            }
                        }
                    }
                    else // email invalid
                    {
                        identity.Label = ExternalLoginStatus.InvalidEmail;
                        if (!string.IsNullOrEmpty(email))
                        {
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The Email Address {Email} Is Invalid Or Does Not Match The Domain Filter Criteria. Login Denied.", email);
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Did Not Return An Email Address To Uniquely Identify The User. The Email Claim Specified Was {EmailCLaimType} And Actual Claim Types Are {Claims}. Login Denied.", httpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", ""), claims);
                        }
                    }
                }

                // manage user
                if (user != null)
                {
                    // create claims identity
                    var _userRoles = httpContext.RequestServices.GetRequiredService<IUserRoleRepository>();
                    identity = UserSecurity.CreateClaimsIdentity(alias, user, _userRoles.GetUserRoles(user.UserId, user.SiteId).ToList());
                    identity.Label = ExternalLoginStatus.Success;

                    // update user
                    user.LastLoginOn = DateTime.UtcNow;
                    user.LastIPAddress = httpContext.Connection.RemoteIpAddress.ToString();
                    _users.UpdateUser(user);

                    // user profile claims
                    if (!string.IsNullOrEmpty(httpContext.GetSiteSettings().GetValue("ExternalLogin:ProfileClaimTypes", "")))
                    {
                        var _settings = httpContext.RequestServices.GetRequiredService<ISettingRepository>();
                        var _profiles = httpContext.RequestServices.GetRequiredService<IProfileRepository>();
                        var profiles = _profiles.GetProfiles(alias.SiteId).ToList();
                        foreach (var mapping in httpContext.GetSiteSettings().GetValue("ExternalLogin:ProfileClaimTypes", "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (mapping.Contains(":"))
                            {
                                var claim = claimsPrincipal.Claims.FirstOrDefault(item => item.Type == mapping.Split(":")[0]);
                                if (claim != null)
                                {
                                    var profile = profiles.FirstOrDefault(item => item.Name == mapping.Split(":")[1]);
                                    if (profile != null)
                                    {
                                        if (!string.IsNullOrEmpty(claim.Value))
                                        {
                                            var setting = _settings.GetSetting(EntityNames.User, user.UserId, profile.Name);
                                            if (setting != null)
                                            {
                                                setting.SettingValue = claim.Value;
                                                _settings.UpdateSetting(setting);
                                            }
                                            else
                                            {
                                                setting = new Setting { EntityName = EntityNames.User, EntityId = user.UserId, SettingName = profile.Name, SettingValue = claim.Value, IsPrivate = profile.IsPrivate };
                                                _settings.AddSetting(setting);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The User Profile {ProfileName} Does Not Exist For The Site. Please Verify Your User Profile Definitions.", mapping.Split(":")[1]);
                                    }
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The User Profile Claim {ClaimType} Does Not Exist. The Valid Claims Are {Claims}.", mapping.Split(":")[0], claims);
                                }
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The User Profile Claim Mapping {Mapping} Is Not Specified Correctly. It Should Be In The Format 'ClaimType:ProfileName'.", mapping);
                            }
                        }
                    }

                    _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "External User Login Successful For {Username} Using Provider {Provider}", user.Username, providerName);
                }
            }
            else // id invalid
            {
                _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Did Not Return An Identifier To Uniquely Identify The User. The Identifier Claim Specified Was {IdentifierCLaimType} And Actual Claim Types Are {Claims}. Login Denied.", httpContext.GetSiteSettings().GetValue("ExternalLogin:IdentifierClaimType", ""), claims);
            }

            return identity;
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
