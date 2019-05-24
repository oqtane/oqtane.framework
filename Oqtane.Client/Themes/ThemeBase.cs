using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Themes
{
    public class ThemeBase : ComponentBase, IThemeControl
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }
        public virtual string Name { get; set; }
        public virtual string Panes { get; set; }

        public string NavigateUrl()
        {
            return Utilities.NavigateUrl(PageState);
        }

        public string NavigateUrl(bool reload)
        {
            return Utilities.NavigateUrl(PageState, reload);
        }

        public string NavigateUrl(string path)
        {
            return Utilities.NavigateUrl(PageState, path);
        }

        public string NavigateUrl(string path, bool reload)
        {
            return Utilities.NavigateUrl(PageState, path, reload);
        }

    }
}
