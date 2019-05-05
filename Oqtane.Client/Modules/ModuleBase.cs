using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.Models;

namespace Oqtane.Modules
{
    public class ModuleBase : ComponentBase, IModuleControl
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        protected Module ModuleState { get; set; }

        public virtual string Title { get { return ""; } }

        public virtual SecurityAccessLevelEnum SecurityAccessLevel { get { return SecurityAccessLevelEnum.View; } set { } } // default security

        public virtual string Actions { get { return ""; } }

        public string NavigateUrl()
        {
            return NavigateUrl(PageState.Page.Path, false);
        }

        public string NavigateUrl(bool reload)
        {
            return NavigateUrl(PageState.Page.Path, reload);
        }

        public string NavigateUrl(string path)
        {
            return NavigateUrl(path, false);
        }

        public string NavigateUrl(string path, bool reload)
        {
            string url = PageState.Alias + path;
            if (reload)
            {
                url += "?reload=true";
            }
            return url;
        }

        public string EditUrl(string action)
        {
            return EditUrl(action, "");
        }

        public string EditUrl(string action, string parameters)
        {
            string url = PageState.Alias + PageState.Page.Path + "?mid=" + ModuleState.ModuleId.ToString();
            if (action != "")
            {
                url += "&ctl=" + action;
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                url += "&" + parameters;
            }
            return url;
        }
    }
}
