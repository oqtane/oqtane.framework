using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Extensions;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class ExternalModel : PageModel
    {
        public IActionResult OnGetAsync(string returnurl)
        {
            returnurl = (returnurl == null) ? "/" : returnurl;
            returnurl = (!returnurl.StartsWith("/")) ? "/" + returnurl : returnurl;

            var providertype = HttpContext.GetSiteSettings().GetValue("ExternalLogin:ProviderType", "");
            if (providertype != "")
            {
                    return new ChallengeResult(providertype, new AuthenticationProperties { RedirectUri = returnurl + (returnurl.Contains("?") ? "&" : "?") + "reload=post" });
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new EmptyResult();
            }
        }

        public IActionResult OnPostAsync(string returnurl)
        {
            if (returnurl == null)
            {
                returnurl = "";
            }
            if (!returnurl.StartsWith("/"))
            {
                returnurl = "/" + returnurl;
            }

            // remove reload parameter
            returnurl = returnurl.ReplaceMultiple(new string[] { "?reload=post", "&reload=post" }, "");

            return LocalRedirect(Url.Content("~" + returnurl));
        }
    }
}
