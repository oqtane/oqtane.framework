using System;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
            var visitorCookieValue = _accessor.HttpContext.Request.Cookies[GetCookieName(siteId)];
            if (visitorCookieValue != null)
            {
                try
                {
                    visitorCookieValue = _protector.Unprotect(visitorCookieValue);
                    if (visitorCookieValue.Contains("|"))
                    {
                        var values = visitorCookieValue.Split('|');
                        if (int.TryParse(values[0], out int VisitorId) && DateTime.TryParse(values[1], null, DateTimeStyles.RoundtripKind, out DateTime Expiry))
                        {
                            return (VisitorId, Expiry);
                        }
                    }
                }
                catch
                {
                    // note that legacy Visitor cookies were not encrypted
                }
            }
            return (-1, DateTime.MinValue);
        }

        public void SetVisitor(int siteId, int visitorId, DateTime expiry)
        {
            // cookie contains visitor id and session expiry date for tracking purposes (ie. "1|yyyy-MM-ddTHH:mm:ss.fffffffK")"
            var visitorCookieValue = $"{visitorId}|{expiry.ToString("o")}";

            _accessor.HttpContext.Response.Cookies.Append(GetCookieName(siteId),
                _protector.Protect(visitorCookieValue),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(10), // 10 years
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax, // must be lax
                    Secure = true, // ensure the cookie is only sent over HTTPS
                    HttpOnly = true // helps mitigate XSS attacks
                });
        }

        public void DeleteVisitor(int siteId)
        {
            _accessor.HttpContext.Response.Cookies.Delete(GetCookieName(siteId));
        }

        private string GetCookieName(int siteId)
        {
            return Constants.VisitorCookiePrefix + siteId.ToString();
        }
    }
}
