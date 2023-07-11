using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Services;
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
        protected ILogService LoggingService { get; set; }

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

        // logging methods
        public async Task Log(Alias alias, LogLevel level, string function, Exception exception, string message, params object[] args)
        {
            LogFunction logFunction;
            if (string.IsNullOrEmpty(function))
            {
                // try to infer from page action
                function = PageState.Action;
            }
            if (!Enum.TryParse(function, out logFunction))
            {
                switch (function.ToLower())
                {
                    case "add":
                        logFunction = LogFunction.Create;
                        break;
                    case "edit":
                        logFunction = LogFunction.Update;
                        break;
                    case "delete":
                        logFunction = LogFunction.Delete;
                        break;
                    case "":
                        logFunction = LogFunction.Read;
                        break;
                    default:
                        logFunction = LogFunction.Other;
                        break;
                }
            }
            await Log(alias, level, logFunction, exception, message, args);
        }

        public async Task Log(Alias alias, LogLevel level, LogFunction function, Exception exception, string message, params object[] args)
        {
            int pageId = PageState.Page.PageId;
            string category = GetType().AssemblyQualifiedName;
            string feature = Utilities.GetTypeNameLastSegment(category, 1);

            await LoggingService.Log(alias, pageId, null, PageState.User?.UserId, category, feature, function, level, exception, message, args);
        }

        public class Logger
        {
            private readonly ModuleBase _moduleBase;

            public Logger(ModuleBase moduleBase)
            {
                _moduleBase = moduleBase;
            }

            public async Task LogTrace(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Trace, "", null, message, args);
            }

            public async Task LogTrace(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Trace, function, null, message, args);
            }

            public async Task LogTrace(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Trace, "", exception, message, args);
            }

            public async Task LogDebug(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Debug, "", null, message, args);
            }

            public async Task LogDebug(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Debug, function, null, message, args);
            }

            public async Task LogDebug(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Debug, "", exception, message, args);
            }

            public async Task LogInformation(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Information, "", null, message, args);
            }

            public async Task LogInformation(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Information, function, null, message, args);
            }

            public async Task LogInformation(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Information, "", exception, message, args);
            }

            public async Task LogWarning(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Warning, "", null, message, args);
            }

            public async Task LogWarning(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Warning, function, null, message, args);
            }

            public async Task LogWarning(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Warning, "", exception, message, args);
            }

            public async Task LogError(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Error, "", null, message, args);
            }

            public async Task LogError(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Error, function, null, message, args);
            }

            public async Task LogError(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Error, "", exception, message, args);
            }

            public async Task LogCritical(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Critical, "", null, message, args);
            }

            public async Task LogCritical(LogFunction function, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Critical, function, null, message, args);
            }

            public async Task LogCritical(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Critical, "", exception, message, args);
            }
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
