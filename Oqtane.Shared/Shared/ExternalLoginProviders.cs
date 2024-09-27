using System.Collections.Generic;
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
                        Name = "Custom",
                        Settings = new Dictionary<string, string>()
                    },
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
                    }
                };

                return providers;
            }
        }
    }
}
