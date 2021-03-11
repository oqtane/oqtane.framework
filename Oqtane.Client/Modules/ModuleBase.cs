using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.Models;
using System.Threading.Tasks;
using Oqtane.Services;
using System;
using Oqtane.Enums;
using Oqtane.UI;
using System.Collections.Generic;
using Microsoft.JSInterop;
using System.Linq;

namespace Oqtane.Modules
{
    public abstract class ModuleBase : ComponentBase, IModuleControl
    {
        private Logger _logger;

        protected Logger logger => _logger ?? (_logger = new Logger(this));

        [Inject]
        protected ILogService LoggingService { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        protected Module ModuleState { get; set; }

        [CascadingParameter]
        protected ModuleInstance ModuleInstance { get; set; }

        // optional interface properties
        public virtual SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.View; } set { } } // default security

        public virtual string Title { get { return ""; } }

        public virtual string Actions { get { return ""; } }

        public virtual bool UseAdminContainer { get { return true; } }

        public virtual List<Resource> Resources { get; set; }

        // base lifecycle method for handling JSInterop script registration

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (Resources != null && Resources.Exists(item => item.ResourceType == ResourceType.Script))
                {
                    var scripts = new List<object>();
                    foreach (Resource resource in Resources.Where(item => item.ResourceType == ResourceType.Script && item.Declaration != ResourceDeclaration.Global))
                    {
                        scripts.Add(new { href = resource.Url, bundle = resource.Bundle ?? "", integrity = resource.Integrity ?? "", crossorigin = resource.CrossOrigin ?? "" });
                    }
                    if (scripts.Any())
                    {
                        var interop = new Interop(JSRuntime);
                        await interop.IncludeScripts(scripts.ToArray());
                    }
                }
            }
        }

        // path method

        public string ModulePath()
        {
            return "Modules/" + GetType().Namespace + "/";
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

        public string EditUrl(string action)
        {
            return EditUrl(ModuleState.ModuleId, action);
        }

        public string EditUrl(string action, string parameters)
        {
            return EditUrl(ModuleState.ModuleId, action, parameters);
        }

        public string EditUrl(int moduleId, string action)
        {
            return EditUrl(moduleId, action, "");
        }

        public string EditUrl(int moduleId, string action, string parameters)
        {
            return EditUrl(PageState.Page.Path, moduleId, action, parameters);
        }

        public string EditUrl(string path, int moduleid, string action, string parameters)
        {
            return Utilities.EditUrl(PageState.Alias.Path, path, moduleid, action, parameters);
        }

        public string ContentUrl(int fileid)
        {
            return ContentUrl(fileid, false);
        }

        public string ContentUrl(int fileid, bool asAttachment)
        {
            return Utilities.ContentUrl(PageState.Alias, fileid, asAttachment);
        }

        public virtual Dictionary<string, string> GetUrlParameters(string parametersTemplate = "")
        {
            var urlParameters = new Dictionary<string, string>();
            string[] templateSegments;
            var parameters = PageState.UrlParameters.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var parameterId = 0;

            if (string.IsNullOrEmpty(parametersTemplate))
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    urlParameters.TryAdd("parameter" + i, parameters[i]);
                }
            }
            else
            {
                templateSegments = parametersTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (parameters.Length == templateSegments.Length)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters.Length > i)
                        {
                            if (templateSegments[i] == parameters[i])
                            {
                                urlParameters.TryAdd("parameter" + parameterId, parameters[i]);
                                parameterId++;
                            }
                            else if (templateSegments[i].StartsWith("{") && templateSegments[i].EndsWith("}"))
                            {
                                var key = templateSegments[i].Replace("{", "");
                                key = key.Replace("}", "");
                                urlParameters.TryAdd(key, parameters[i]);
                            }
                            else
                            {
                                i = parameters.Length;
                                urlParameters.Clear();
                            }
                        }
                    }
                }
            }

            return urlParameters;
        }

        // user feedback methods
        public void AddModuleMessage(string message, MessageType type)
        {
            ModuleInstance.AddModuleMessage(message, type);
        }

        public void ClearModuleMessage()
        {
            ModuleInstance.AddModuleMessage("", MessageType.Undefined);
        }

        public void ShowProgressIndicator()
        {
            ModuleInstance.ShowProgressIndicator();
        }

        public void HideProgressIndicator()
        {
            ModuleInstance.HideProgressIndicator();
        }

        // logging methods
        public async Task Log(Alias alias, LogLevel level, string function, Exception exception, string message, params object[] args)
        {
            int pageId = ModuleState.PageId;
            int moduleId = ModuleState.ModuleId;
            int? userId = null;
            if (PageState.User != null)
            {
                userId = PageState.User.UserId;
            }
            string category = GetType().AssemblyQualifiedName;
            string feature = Utilities.GetTypeNameLastSegment(category, 1);
            LogFunction logFunction;
            if (string.IsNullOrEmpty(function))
            {
                function = PageState.Action;
            }
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

                default:
                    logFunction = LogFunction.Read;
                    break;
            }
            if (feature == "Login")
            {
                logFunction = LogFunction.Security;
            }
            await LoggingService.Log(alias, pageId, moduleId, userId, category, feature, logFunction, level, exception, message, args);
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

            public async Task LogTrace(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Trace, "", exception, message, args);
            }

            public async Task LogDebug(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Debug, "", null, message, args);
            }

            public async Task LogDebug(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Debug, "", exception, message, args);
            }

            public async Task LogInformation(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Information, "", null, message, args);
            }

            public async Task LogInformation(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Information, "", exception, message, args);
            }

            public async Task LogWarning(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Warning, "", null, message, args);
            }

            public async Task LogWarning(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Warning, "", exception, message, args);
            }

            public async Task LogError(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Error, "", null, message, args);
            }

            public async Task LogError(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Error, "", exception, message, args);
            }

            public async Task LogCritical(string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Critical, "", null, message, args);
            }

            public async Task LogCritical(Exception exception, string message, params object[] args)
            {
                await _moduleBase.Log(null, LogLevel.Critical, "", exception, message, args);
            }
        }
    }
}
