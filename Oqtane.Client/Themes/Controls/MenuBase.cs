using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Themes.Controls
{
    public class MenuBase : ThemeControlBase
    {
        private List<Page> _menuPages;

        protected IEnumerable<Page> MenuPages => _menuPages ?? (_menuPages = GetMenuPages().ToList());

        protected string GetTarget(Page page)
        {
            return page.Url.StartsWith("http") ? "_new" : string.Empty;
        }

        protected string GetUrl(Page page)
        {
            return string.IsNullOrEmpty(page.Url) ? NavigateUrl(page.Path) : page.Url;
        }

        private IEnumerable<Page> GetMenuPages()
        {
            var securityLevel = int.MaxValue;
            foreach (Page p in PageState.Pages.Where(item => item.IsNavigation && !item.IsDeleted))
            {
                if (p.Level <= securityLevel && UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, p.Permissions))
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
