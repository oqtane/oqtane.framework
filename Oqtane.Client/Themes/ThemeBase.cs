using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Oqtane.Themes
{
    public abstract class ThemeBase : ComponentBase, IThemeControl
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected SiteState SiteState { get; set; }

        [CascadingParameter]
        protected PageState PageState { get; set; }

        // optional interface properties
        public virtual string Name { get; set; }
        public virtual string Thumbnail { get; set; }
        public virtual string Panes { get; set; }
        public virtual List<Resource> Resources { get; set; }

        // base lifecycle method for handling JSInterop script registration

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                List<Resource> resources = null;
                var type = GetType();
                if (type.BaseType == typeof(ThemeBase))
                {
                    if (PageState.Page.Resources != null)
                    {
                        resources = PageState.Page.Resources.Where(item => item.ResourceType == ResourceType.Script && item.Level != ResourceLevel.Site && item.Namespace == type.Namespace).ToList();
                    }
                }
                else // themecontrolbase, containerbase
                {
                    if (Resources != null)
                    {
                        resources = Resources.Where(item => item.ResourceType == ResourceType.Script).ToList();
                    }
                }
                if (resources != null && resources.Any())
                {
                    var interop = new Interop(JSRuntime);
                    var scripts = new List<object>();
                    var inline = 0;
                    foreach (Resource resource in resources)
                    {
                        if (!string.IsNullOrEmpty(resource.Url))
                        {
                            var url = (resource.Url.Contains("://")) ? resource.Url : PageState.Alias.BaseUrl + resource.Url;
                            scripts.Add(new { href = url, bundle = resource.Bundle ?? "", integrity = resource.Integrity ?? "", crossorigin = resource.CrossOrigin ?? "", es6module = resource.ES6Module, location = resource.Location.ToString().ToLower() });
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

        public void SetPageTitle(string title)
        {
            SiteState.Properties.PageTitle = title;
        }

        // note - only supports links and meta tags - not scripts
        public void AddHeadContent(string content)
        {
            SiteState.AppendHeadContent(content);
        }

        public void AddScript(Resource resource)
        {
            resource.ResourceType = ResourceType.Script;
            if (Resources == null) Resources = new List<Resource>();
            if (!Resources.Any(item => (!string.IsNullOrEmpty(resource.Url) && item.Url == resource.Url) || (!string.IsNullOrEmpty(resource.Content) && item.Content == resource.Content)))
            {
                Resources.Add(resource);
            }
        }

        public async Task ScrollToPageTop()
        {
            var interop = new Interop(JSRuntime);
            await interop.ScrollTo(0, 0, "smooth");
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
