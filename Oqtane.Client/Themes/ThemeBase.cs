using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.UI;
using System;
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
                    var interop = new Interop(JSRuntime);
                    var scripts = new List<object>();
                    var inline = 0;
                    foreach (Resource resource in Resources.Where(item => item.ResourceType == ResourceType.Script))
                    {
                        if (!string.IsNullOrEmpty(resource.Url))
                        {
                            var url = (resource.Url.Contains("://")) ? resource.Url : PageState.Alias.BaseUrl + resource.Url;
                            scripts.Add(new { href = url, bundle = resource.Bundle ?? "", integrity = resource.Integrity ?? "", crossorigin = resource.CrossOrigin ?? "", es6module = resource.ES6Module });
                        }
                        else
                        {
                            inline += 1;
                            await interop.IncludeScript(GetType().Namespace.ToLower() + inline.ToString(), "", "", "", resource.Content, resource.Location.ToString().ToLower());
                        }
                    }
                    if (scripts.Any())
                    {
                        await interop.IncludeScripts(scripts.ToArray());
                    }
                }
            }
        }

        // path method

        public string ThemePath()
        {
            return PageState?.Alias.BaseUrl + "/Themes/" + GetType().Namespace + "/";
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

        public string NavigateUrl(bool refresh)
        {
            return NavigateUrl(PageState.Page.Path, refresh);
        }

        public string NavigateUrl(string path, bool refresh)
        {
            return Utilities.NavigateUrl(PageState.Alias.Path, path, refresh ? "refresh" : "");
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

        public string FileUrl(string folderpath, string filename)
        {
            return FileUrl(folderpath, filename, false);
        }

        public string FileUrl(string folderpath, string filename, bool download)
        {
            return Utilities.FileUrl(PageState.Alias, folderpath, filename, download);
        }
        public string FileUrl(int fileid)
        {
            return FileUrl(fileid, false);
        }

        public string FileUrl(int fileid, bool download)
        {
            return Utilities.FileUrl(PageState.Alias, fileid, download);
        }

        public string ImageUrl(int fileid, int width, int height)
        {
            return ImageUrl(fileid, width, height, "");
        }

        public string ImageUrl(int fileid, int width, int height, string mode)
        {
            return ImageUrl(fileid, width, height, mode, "", "", 0, false);
        }

        public string ImageUrl(int fileid, int width, int height, string mode, string position, string background, int rotate, bool recreate)
        {
            return Utilities.ImageUrl(PageState.Alias, fileid, width, height, mode, position, background, rotate, recreate);
        }

        [Obsolete("ContentUrl(int fileId) is deprecated. Use FileUrl(int fileId) instead.", false)]
        public string ContentUrl(int fileid)
        {
            return ContentUrl(fileid, false);
        }

        [Obsolete("ContentUrl(int fileId, bool asAttachment) is deprecated. Use FileUrl(int fileId, bool download) instead.", false)]
        public string ContentUrl(int fileid, bool asAttachment)
        {
            return Utilities.FileUrl(PageState.Alias, fileid, asAttachment);
        }
    }
}
