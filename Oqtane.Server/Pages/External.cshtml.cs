using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Extensions;

namespace Oqtane.Pages
{
    public class ExternalModel : PageModel
    {
        public IActionResult OnGetAsync(string returnurl)
        {
            returnurl = (returnurl == null) ? "/" : returnurl;
            returnurl = (!returnurl.StartsWith("/")) ? "/" + returnurl : returnurl;

            var providertype = HttpContext.GetSiteSettings().GetValue("ExternalLogin:ProviderType", "");
            if (providertype != "")
            {
                    return new ChallengeResult(providertype, new AuthenticationProperties { RedirectUri = returnurl });
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new EmptyResult();
            }
        }
    }
}
