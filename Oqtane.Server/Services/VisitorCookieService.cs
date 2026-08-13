using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public interface IVisitorCookieService
    {
        /// <summary>
        /// Get the visitor cookie
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns>VisitorId</returns>
        (int VisitorId, DateTime Expiry) GetVisitor(int siteId);

        /// <summary>
        /// Set the visitor cookie
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="visitorId"></param>
        /// <param name="expiry"></param>
        void SetVisitor(int siteId, int visitorId, DateTime expiry);

        /// <summary>
        /// Delete the visitor cookie
        /// </summary>
        /// <param name="siteId"></param>
        void DeleteVisitor(int siteId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class VisitorCookieService : IVisitorCookieService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IDataProtector _protector;

        public VisitorCookieService(IHttpContextAccessor accessor, IDataProtectionProvider dataProtectionProvider)
        {
            _accessor = accessor;
            _protector = dataProtectionProvider.CreateProtector("Oqtane.Visitor");
        }

        public (int VisitorId, DateTime Expiry) GetVisitor(int siteId)
        {
            var visitorCookieName = Constants.VisitorCookiePrefix + siteId.ToString();
            var visitorCookieValue = _accessor.HttpContext.Request.Cookies[visitorCookieName];
            if (visitorCookieValue != null)
            {
                try
                {
                    visitorCookieValue = _protector.Unprotect(visitorCookieValue);
                    if (visitorCookieValue.Contains("|"))
                    {
                        var values = visitorCookieValue.Split('|');
                        if (int.TryParse(values[0], out int VisitorId) && DateTime.TryParseExact(values[1], "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime Expiry))
                        {
                            return (VisitorId, Expiry);
                        }
                    }
                }
                catch
                {
                    // cryptographic exception - note that legacy Visitor cookies were not encrypted
                }
            }
            return (-1, DateTime.MinValue);
        }

        public void SetVisitor(int siteId, int visitorId, DateTime expiry)
        {
            var visitorCookieName = Constants.VisitorCookiePrefix + siteId.ToString();
            var visitorCookieValue = $"{visitorId}|{expiry.ToString("M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}";

            _accessor.HttpContext.Response.Cookies.Append(visitorCookieName,
                _protector.Protect(visitorCookieValue),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(10), // 10 years
                    IsEssential = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax, // must be lax
                    Secure = true, // ensure the cookie is only sent over HTTPS
                    HttpOnly = true // helps mitigate XSS attacks
                });
        }

        public void DeleteVisitor(int siteId)
        {
            var visitorCookieName = Constants.VisitorCookiePrefix + siteId.ToString();
            _accessor.HttpContext.Response.Cookies.Delete(visitorCookieName);
        }
    }
}
