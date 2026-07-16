using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using Oqtane.Models;
using Oqtane.UI;

namespace Oqtane.Themes.Controls
{
    public abstract class MenuItemsBase : MenuBase
    {
        [Inject]
        protected IStringLocalizerFactory LocalizerFactory { get; set; }

        [Inject]
        protected IStringLocalizer<SharedResources> SharedLocalizer { get; set; }

        [Parameter()]
        public Page ParentPage { get; set; }

        [Parameter()]
        public IEnumerable<Page> Pages { get; set; }

        private IStringLocalizer _themeLocalizer;
        private string _themeType;

        protected override void OnParametersSet()
        {
            if (_themeType != PageState.Page.ThemeType)
            {
                _themeType = PageState.Page.ThemeType;
                try
                {
                    _themeLocalizer = LocalizerFactory.Create(_themeType);
                }
                catch
                {
                    _themeLocalizer = null; // theme type could not be resolved
                }
            }
        }

        protected virtual string GetName(Page page)
        {
            // page names are localized using the page name as the static resource key - the current theme's resources are searched first
            // so that theme developers can provide their own translations, then shared resources (following the same convention as the Admin Dashboard)
            if (_themeLocalizer != null)
            {
                var name = _themeLocalizer[page.Name];
                if (!name.ResourceNotFound)
                {
                    return name;
                }
            }
            return SharedLocalizer[page.Name];
        }

        protected IEnumerable<Page> GetChildPages()
        {
            return Pages
                .Where(e => e.ParentId == ParentPage?.PageId)
                .OrderBy(e => e.Order)
                .AsEnumerable();
        }
    }
}
