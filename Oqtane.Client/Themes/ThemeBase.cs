using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Shared;
using System.Threading.Tasks;

namespace Oqtane.Themes
{
    public class ThemeBase : ComponentBase, IThemeControl
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        protected PageState PageState { get; set; }
        public virtual string Panes { get; set; }

        public string ThemePath()
        {
            return "Themes/" + this.GetType().Namespace + "/";
        }

        public async Task IncludeCSS(string Url)
        {
            if (!Url.StartsWith("http"))
            {
                Url = ThemePath() + Url;
            }
            var interop = new Interop(JSRuntime);
            await interop.IncludeCSS("Theme", Url);
        }

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
