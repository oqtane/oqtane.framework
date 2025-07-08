using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Shared
{
    public class ExternalLoginProviders
    {
        public static List<ExternalLoginProvider> Providers
        {
            get
            {
                var providers = new List<ExternalLoginProvider>
                {
                    new ExternalLoginProvider
                    {
                        Name = "<Custom>",
                        Settings = new Dictionary<string, string>()
                    },
                    // OIDC
                    new ExternalLoginProvider
                    {
                        Name = "Microsoft Entra",
                        Settings = new Dictionary<string, string>()
                        {
                            { "ExternalLogin:ProviderUrl", "https://entra.microsoft.com" },
                            { "ExternalLogin:ProviderType", "oidc" },
                            { "ExternalLogin:ProviderName", "Microsoft Entra" },
                            { "ExternalLogin:Authority", "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0" },
                            { "ExternalLogin:ClientId", "YOUR CLIENT ID" },
                            { "ExternalLogin:ClientSecret", "YOUR CLIENT SECRET" }
                        }
                    },
                    new ExternalLoginProvider
                    {
                        Name = "Auth0 (by Okta)",
                        Settings = new Dictionary<string, string>()
                        {
                            { "ExternalLogin:ProviderUrl", "https://auth0.com/docs/get-started" },
                            { "ExternalLogin:ProviderType", "oidc" },
                            { "ExternalLogin:ProviderName", "Auth0" },
                            { "ExternalLogin:Authority", "YOUR DOMAIN" },
                            { "ExternalLogin:ClientId", "YOUR CLIENT ID" },
                            { "ExternalLogin:ClientSecret", "YOUR CLIENT SECRET" }
                        }
                    },
                    // OAuth2
                    new ExternalLoginProvider
                    {
                        Name = "GitHub",
                        Settings = new Dictionary<string, string>()
                        {
                            { "ExternalLogin:ProviderUrl", "https://github.com/settings/developers#oauth-apps" },
                            { "ExternalLogin:ProviderType", "oauth2" },
                            { "ExternalLogin:ProviderName", "GitHub" },
                            { "ExternalLogin:AuthorizationUrl", "https://github.com/login/oauth/authorize" },
                            { "ExternalLogin:TokenUrl", "https://github.com/login/oauth/access_token" },
                            { "ExternalLogin:UserInfoUrl", "https://api.github.com/user/emails" },
                            { "ExternalLogin:ClientId", "YOUR CLIENT ID" },
                            { "ExternalLogin:ClientSecret", "YOUR CLIENT SECRET" },
                            { "ExternalLogin:Scopes", "user:email" },
                            { "ExternalLogin:IdentifierClaimType", "email" },
                            { "ExternalLogin:DomainFilter", "!users.noreply.github.com" }
                        }
                    },
                    new ExternalLoginProvider
                    {
                        Name = "Facebook",
                        Settings = new Dictionary<string, string>()
                        {
                            { "ExternalLogin:ProviderUrl", "https://developers.facebook.com" },
                            { "ExternalLogin:ProviderType", "oauth2" },
                            { "ExternalLogin:ProviderName", "Facebook" },
                            { "ExternalLogin:AuthorizationUrl", "https://www.facebook.com/v23.0/dialog/oauth" },
                            { "ExternalLogin:TokenUrl", "https://graph.facebook.com/v23.0/oauth/access_token" },
                            { "ExternalLogin:UserInfoUrl", "https://graph.facebook.com/v23.0/me?fields=id,name,email" },
                            { "ExternalLogin:ClientId", "YOUR CLIENT ID" },
                            { "ExternalLogin:ClientSecret", "YOUR CLIENT SECRET" },
                            { "ExternalLogin:Scopes", "public_profile,email" },
                            { "ExternalLogin:IdentifierClaimType", "id" },
                            { "ExternalLogin:NameClaimType", "name" },
                            { "ExternalLogin:EmailClaimType", "email" }
                        }
                    }
                };

                return providers.OrderBy(item => item.Name).ToList();
            }
        }
    }
}
