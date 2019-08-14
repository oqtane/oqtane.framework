using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.Models;

namespace Oqtane.Themes
{
    public class ContainerBase : ComponentBase, IContainerControl
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        protected Module ModuleState { get; set; }

        public virtual string Name { get; set; }

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
        public string EditUrl(string action, string parameters)
        {
            return EditUrl(ModuleState.ModuleId, action, parameters);
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
