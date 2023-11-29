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
            builder.AddSiteNamedOptions<CookieAuthenticationOptions>(Constants.AuthenticationScheme, (options, alias, sitesettings) =>
            {
                options.Cookie.Name = sitesettings.GetValue("LoginOptions:CookieName", ".AspNetCore.Identity.Application");
                string cookieExpStr = sitesettings.GetValue("LoginOptions:CookieExpiration", "");
                if (!string.IsNullOrEmpty(cookieExpStr) && TimeSpan.TryParse(cookieExpStr, out TimeSpan cookieExpTS))
                {
                    options.Cookie.Expiration = cookieExpTS;
                    options.ExpireTimeSpan = cookieExpTS;
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
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost; // recommended as most secure

                    // cookie config is required to avoid Correlation Failed errors
                    options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                    // site options
                    options.Authority = sitesettings.GetValue("ExternalLogin:Authority", "");
                    options.MetadataAddress = sitesettings.GetValue("ExternalLogin:MetadataUrl", "");
                    options.ClientId = sitesettings.GetValue("ExternalLogin:ClientId", "");
                    options.ClientSecret = sitesettings.GetValue("ExternalLogin:ClientSecret", "");
                    options.ResponseType = sitesettings.GetValue("ExternalLogin:AuthResponseType", "code"); // default is authorization code flow
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
            var claims = "";
            var id = "";
            var name = "";
            var email = "";

            if (context.Options.UserInformationEndpoint != "")
            {
                try
                {
                    // call user information endpoint using access token
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    claims = await response.Content.ReadAsStringAsync();

                    // get claim types
                    var idClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:IdentifierClaimType", "");
                    var nameClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:NameClaimType", "");
                    var emailClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", "");

                    // some user endpoints can return multiple objects (ie. GitHub) so convert single object to array (if necessary)
                    var jsonclaims = claims;
                    if (!jsonclaims.StartsWith("[") && !jsonclaims.EndsWith("]"))
                    {
                        jsonclaims = "[" + jsonclaims + "]"; 
                    }

                    // parse claim values
                    JsonNode items = JsonNode.Parse(jsonclaims)!;
                    foreach (var item in items.AsArray())
                    {
                        // id claim is required
                        if (!string.IsNullOrEmpty(idClaimType) && item[idClaimType] != null)
                        {
                            id = item[idClaimType].ToString();

                            // name claim is optional
                            if (!string.IsNullOrEmpty(nameClaimType))
                            {
                                if (item[nameClaimType] != null)
                                {
                                    name = item[nameClaimType].ToString();
                                }
                                else
                                {
                                    id = ""; // name claim was specified but was not provided
                                }
                            }

                            // email claim is optional
                            if (!string.IsNullOrEmpty(emailClaimType))
                            {
                                if (item[emailClaimType] != null && EmailValid(item[emailClaimType].ToString(), context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
                                {
                                    email = item[emailClaimType].ToString().ToLower();
                                }
                                else
                                {
                                    id = ""; // email claim was specified but was not provided or is invalid
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(id))
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, ex, "An Error Occurred Accessing The User Info Endpoint - {Error}", ex.Message);
                }
            }

            // validate user
            var identity = await ValidateUser(id, name, email, claims, context.HttpContext, context.Principal);
            if (identity.Label == ExternalLoginStatus.Success)
            {
                identity.AddClaim(new Claim("access_token", context.AccessToken));
                context.Principal = new ClaimsPrincipal(identity);
            }

            // pass properties to OnTicketReceived
            context.Properties.SetParameter("status", identity.Label);
            context.Properties.SetParameter("redirecturl", context.Properties.RedirectUri);

            // set cookie expiration
            string cookieExpStr = context.HttpContext.GetSiteSettings().GetValue("LoginOptions:CookieExpiration", "");
            if (!string.IsNullOrEmpty(cookieExpStr) && TimeSpan.TryParse(cookieExpStr, out TimeSpan cookieExpTS))
            {
                context.Properties.ExpiresUtc = DateTime.Now.Add(cookieExpTS);
                context.Properties.IsPersistent = true;
            }
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
            var claims = "";
            var id = "";
            var name = "";
            var email = "";

            // serialize claims
            foreach (var claim in context.Principal.Claims)
            {
                claims += "\"" + claim.Type + "\":\"" + claim.Value + "\",";
            }
            claims = "{" + claims.Substring(0, claims.Length - 1) + "}";

            // get claim types
            var idClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:IdentifierClaimType", "");
            var nameClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:NameClaimType", "");
            var emailClaimType = context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:EmailClaimType", "");

            // parse claim values
            id = context.Principal.FindFirstValue(idClaimType); // required
            if (!string.IsNullOrEmpty(nameClaimType))
            {
                if (context.Principal.FindFirstValue(nameClaimType) != null)
                {
                    name = context.Principal.FindFirstValue(nameClaimType);
                }
                else
                {
                    id = ""; // name claim was specified but was not provided
                }
            }
            if (!string.IsNullOrEmpty(emailClaimType))
            {
                if (context.Principal.FindFirstValue(emailClaimType) != null && EmailValid(context.Principal.FindFirstValue(emailClaimType), context.HttpContext.GetSiteSettings().GetValue("ExternalLogin:DomainFilter", "")))
                {
                    email = context.Principal.FindFirstValue(emailClaimType);
                }
                else
                {
                    id = ""; // email claim was specified but was not provided or is invalid
                }
            }

            // validate user
            var identity = await ValidateUser(id, name, email, claims, context.HttpContext, context.Principal);
            if (identity.Label == ExternalLoginStatus.Success)
            {
                // include access token
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

        private static async Task<ClaimsIdentity> ValidateUser(string id, string name, string email, string claims, HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            var _logger = httpContext.RequestServices.GetRequiredService<ILogManager>();
            ClaimsIdentity identity = new ClaimsIdentity(Constants.AuthenticationScheme);
            // use identity.Label as a temporary location to store validation status information

            // review claims feature (for testing - external login is disabled)
            if (bool.Parse(httpContext.GetSiteSettings().GetValue("ExternalLogin:ReviewClaims", "false")))
            {
                _logger.Log(LogLevel.Information, "ExternalLogin", Enums.LogFunction.Security, "Provider Returned The Following Claims: {Claims}", claims);
                identity.Label = ExternalLoginStatus.ReviewClaims;
                return identity;
            }

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
                    user.SiteId = alias.SiteId;
                }
                else
                {
                    bool duplicates = false;
                    if (!string.IsNullOrEmpty(email))
                    {
                        try
                        {
                            identityuser = await _identityUserManager.FindByEmailAsync(email);
                        }
                        catch // FindByEmailAsync will throw an error if the email matches multiple user accounts
                        {                                
                            duplicates = true;
                        }
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
                                // user identifiers
                                var username = "";
                                var emailaddress = "";
                                var displayname = "";
                                bool emailconfirmed = false;

                                if (!string.IsNullOrEmpty(email)) // email claim provided
                                {
                                    username = email;
                                    emailaddress = email;
                                    displayname = (!string.IsNullOrEmpty(name)) ? name : email;
                                    emailconfirmed = true;
                                }
                                else if (!string.IsNullOrEmpty(name)) // name claim provided
                                {
                                    username = name.ToLower().Replace(" ", "") + DateTime.UtcNow.ToString("mmss");
                                    emailaddress = ""; // unknown - will need to be requested from user later
                                    displayname = name;
                                }
                                else // neither email nor name provided
                                {
                                    username = Guid.NewGuid().ToString("N"); 
                                    emailaddress = ""; // unknown - will need to be requested from user later
                                    displayname = username;
                                }

                                identityuser = new IdentityUser();
                                identityuser.UserName = username;
                                identityuser.Email = emailaddress;
                                identityuser.EmailConfirmed = emailconfirmed;

                                // generate password based on random date and punctuation ie. Jan-23-1981+14:43:12!
                                Random rnd = new Random();
                                var date = DateTime.UtcNow.AddDays(-rnd.Next(50 * 365)).AddHours(rnd.Next(0, 24)).AddMinutes(rnd.Next(0, 60)).AddSeconds(rnd.Next(0, 60));
                                var password = date.ToString("MMM-dd-yyyy+HH:mm:ss", CultureInfo.InvariantCulture) + (char)rnd.Next(33, 47);

                                var result = await _identityUserManager.CreateAsync(identityuser, password);
                                if (result.Succeeded)
                                {
                                    user = new User
                                    {
                                        SiteId = alias.SiteId,
                                        Username = username,
                                        DisplayName = displayname,
                                        Email = emailaddress,
                                        LastLoginOn = null,
                                        LastIPAddress = ""
                                    };
                                    user = _users.AddUser(user);

                                    if (user != null)
                                    {
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            var _notifications = httpContext.RequestServices.GetRequiredService<INotificationRepository>();
                                            string url = httpContext.Request.Scheme + "://" + alias.Name;
                                            string body = "You Recently Used An External Account To Sign In To Our Site.\n\n" + url + "\n\nThank You!";
                                            var notification = new Notification(user.SiteId, user, "User Account Notification", body);
                                            _notifications.AddNotification(notification);
                                        }

                                        // add user login
                                        await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType + ":" + user.SiteId.ToString(), id, providerName));

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
                            if (bool.Parse(httpContext.GetSiteSettings().GetValue("ExternalLogin:VerifyUsers", "true")))
                            {
                                // external login using existing user account - verification required
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
                                // external login using existing user account - link automatically
                                user = _users.GetUser(identityuser.UserName);
                                user.SiteId = alias.SiteId;

                                var _notifications = httpContext.RequestServices.GetRequiredService<INotificationRepository>();
                                string url = httpContext.Request.Scheme + "://" + alias.Name;
                                string body = "You Recently Used An External Account To Sign In To Our Site.\n\n" + url + "\n\nThank You!";
                                var notification = new Notification(user.SiteId, user, "User Account Notification", body);
                                _notifications.AddNotification(notification);

                                // add user login
                                await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(providerType + ":" + user.SiteId.ToString(), id, providerName));

                                _logger.Log(user.SiteId, LogLevel.Information, "ExternalLogin", Enums.LogFunction.Create, "External Login Linkage Created For User {Username} And Provider {Provider}", user.Username, providerName);
                            }
                        }
                        else
                        {
                            // provider keys do not match
                            identity.Label = ExternalLoginStatus.ProviderKeyMismatch;
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Key Does Not Match For User {Username}. Login Denied.", identityuser.UserName);
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

                    // external roles
                    if (!string.IsNullOrEmpty(httpContext.GetSiteSettings().GetValue("ExternalLogin:RoleClaimType", "")))
                    {
                        if (claimsPrincipal.Claims.Any(item => item.Type == ClaimTypes.Role))
                        {
                            foreach (var claim in claimsPrincipal.Claims.Where(item => item.Type == ClaimTypes.Role))
                            {
                                if (!identity.Claims.Any(item => item.Type == ClaimTypes.Role && item.Value == claim.Value))
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                                }
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The Role Claim {ClaimType} Does Not Exist. Please Use The Review Claims Feature To View The Claims Returned By Your Provider.", httpContext.GetSiteSettings().GetValue("ExternalLogin:RoleClaimType", ""));
                        }
                    }

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
                                    _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "The User Profile Claim {ClaimType} Does Not Exist. Please Use The Review Claims Feature To View The Claims Returned By Your Provider.", mapping.Split(":")[0]);
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
            else // claims invalid
            {
                identity.Label = ExternalLoginStatus.MissingClaims;
                _logger.Log(LogLevel.Error, "ExternalLogin", Enums.LogFunction.Security, "Provider Did Not Return All Of The Claims Types Specified Or Email Address Does Not Saitisfy Domain Filter. The Actual Claims Returned Were {Claims}. Login Was Denied.", claims);
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
