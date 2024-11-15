using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ServerLocalizationCookieService : ILocalizationCookieService
    {
        private readonly IHttpContextAccessor _accessor;

        public ServerLocalizationCookieService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task SetLocalizationCookieAsync(string culture)
        {
            var localizationCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));

            _accessor.HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, localizationCookieValue, new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddYears(365),
                SameSite = SameSiteMode.Lax,
                Secure = true, // Ensure the cookie is only sent over HTTPS
                HttpOnly = false // cookie is updated using JS Interop in Interactive render mode
            });

            return Task.CompletedTask;
        }
    }
}
