using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.UI;

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

        public string NavigateUrl(string path)
        {
            return NavigateUrl(path, "");
        }

        public string NavigateUrl(string path, string parameters)
        {
            return Utilities.NavigateUrl(PageState.Alias.Path, path, parameters);
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

        public string ContentUrl(int fileid)
        {
            return Utilities.ContentUrl(PageState.Alias.Path, fileid);
        }
    }
}
