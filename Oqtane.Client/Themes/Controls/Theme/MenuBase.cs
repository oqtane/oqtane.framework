using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Themes.Controls
{
    public class MenuBase : ThemeControlBase
    {
        protected IEnumerable<Page> MenuPages => GetMenuPages().ToList();

        protected string GetTarget(Page page)
        {
            return page.Url != null && page.Url.StartsWith("http") ? "_new" : string.Empty;
        }

        protected string GetUrl(Page page)
        {
            if (page.IsClickable)
            {
                var url = string.IsNullOrEmpty(page.Url) ? NavigateUrl(page.Path) : page.Url;
                if (PageState.QueryString.ContainsKey("method"))
                {
                    url += ((url.Contains("?")) ? "&" : "?") + "method=" + PageState.QueryString["method"];
                }
                return url;
            }
            else
            {
                return "javascript:void(0)";
            }
        }

        private IEnumerable<Page> GetMenuPages()
        {
            var securityLevel = int.MaxValue;
            foreach (Page p in PageState.Pages.Where(item => item.IsNavigation))
            {
                if (p.Level <= securityLevel && UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, p.PermissionList))
                {
                    securityLevel = int.MaxValue;
                    yield return p;
                }
                else
                {
                    if (securityLevel == int.MaxValue)
                    {
                        securityLevel = p.Level;
                    }
                }
            }
        }
    }
}
