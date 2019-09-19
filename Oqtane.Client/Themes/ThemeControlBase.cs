using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Themes
{
    public class ThemeControlBase : ComponentBase
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        public string NavigateUrl()
        {
            return NavigateUrl(PageState.Page.Path);
        }

        public string NavigateUrl(Reload reload)
        {
            return NavigateUrl(PageState.Page.Path, reload);
        }

        public string NavigateUrl(string path)
        {
            return NavigateUrl(path, "", Reload.None);
        }

        public string NavigateUrl(string path, Reload reload)
        {
            return NavigateUrl(path, "", reload);
        }

        public string NavigateUrl(string path, string parameters)
        {
            return Utilities.NavigateUrl(PageState.Alias.Path, path, parameters, Reload.None);
        }

        public string NavigateUrl(string path, string parameters, Reload reload)
        {
            return Utilities.NavigateUrl(PageState.Alias.Path, path, parameters, reload);
        }

        public string EditUrl(int moduleid, string action)
        {
            return EditUrl(moduleid, action, "");
        }

        public string EditUrl(int moduleid, string action, string parameters)
        {
            return EditUrl(PageState.Page.Path, moduleid, action, parameters);
        }

        public string EditUrl(string path, int moduleid, string action, string parameters)
        {
            return Utilities.EditUrl(PageState.Alias.Path, path, moduleid, action, parameters);
        }
    }
}
