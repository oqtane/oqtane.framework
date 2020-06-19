using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.Themes
{
    public abstract class ThemeBase : ComponentBase, IThemeControl
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        // optional interface properties

        [CascadingParameter]
        protected PageState PageState { get; set; }
        public virtual string Name { get; set; }
        public virtual string Thumbnail { get; set; }
        public virtual string Panes { get; set; }
        public virtual List<Resource> Resources { get; set; }

        // base lifecycle method for handling JSInterop script registration

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (Resources != null && Resources.Exists(item => item.ResourceType == ResourceType.Script))
                {
                    var scripts = new List<object>();
                    foreach (Resource resource in Resources.Where(item => item.ResourceType == ResourceType.Script))
                    {
                        scripts.Add(new { href = resource.Url, bundle = resource.Bundle ?? "", integrity = resource.Integrity ?? "", crossorigin = resource.CrossOrigin ?? "" });
                    }
                    var interop = new Interop(JSRuntime);
                    await interop.IncludeScripts(scripts.ToArray());
                }
            }
        }

        // path method

        public string ThemePath()
        {
            return "Themes/" + GetType().Namespace + "/";
        }

        // url methods

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
            return Utilities.ContentUrl(PageState.Alias, fileid);
        }
    }
}
