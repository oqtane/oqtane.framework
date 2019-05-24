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

        public string EditUrl(string action)
        {
            return Utilities.EditUrl(PageState, ModuleState, action, "");
        }

        public string EditUrl(string action, string parameters)
        {
            return Utilities.EditUrl(PageState, ModuleState, action, parameters);
        }
    }
}
