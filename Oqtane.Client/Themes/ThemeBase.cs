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
            return NavigateUrl(PageState.Page.Path);
        }

        public string NavigateUrl(string path)
        {
            return Utilities.NavigateUrl(PageState.Alias.Path, path);
        }

    }
}
