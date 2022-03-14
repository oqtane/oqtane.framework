using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    internal class SiteAuthenticationService<TAlias> : IAuthenticationService
        where TAlias : class, IAlias, new()
    {
        private readonly IAuthenticationService _inner;

        public SiteAuthenticationService(IAuthenticationService inner)
        {
            _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        private static void AddTenantIdentifierToProperties(HttpContext context, ref AuthenticationProperties properties)
        {
            // add site identifier to the authentication properties so on the callback we can use it to set context
            var alias = context.GetAlias();
            if (alias != null)
            {
                properties ??= new AuthenticationProperties();
                if (!properties.Items.Keys.Contains(Constants.SiteToken))
                {
                    properties.Items.Add(Constants.SiteToken, alias.SiteKey);
                }
            }
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
            => _inner.AuthenticateAsync(context, scheme);

        public async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentifierToProperties(context, ref properties);
            await _inner.ChallengeAsync(context, scheme, properties);
        }

        public async Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentifierToProperties(context, ref properties);
            await _inner.ForbidAsync(context, scheme, properties);
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            AddTenantIdentifierToProperties(context, ref properties);
            await _inner.SignInAsync(context, scheme, principal, properties);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentifierToProperties(context, ref properties);
            await _inner.SignOutAsync(context, scheme, properties);
        }
    }
}
