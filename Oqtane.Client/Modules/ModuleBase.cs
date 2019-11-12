using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using Oqtane.Services;
using System;

namespace Oqtane.Modules
{
    public class ModuleBase : ComponentBase, IModuleControl
    {
        public Logger logger { get; set; }

        public ModuleBase()
        {
            this.logger = new Logger(this);
        }

        [Inject]
        protected ILogService LoggingService { get; set; }

        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        protected Module ModuleState { get; set; }

        [CascadingParameter] 
        protected ModuleInstance ModuleInstance { get; set; }

        protected ModuleDefinition ModuleDefinition
        {
            get
            {
                return PageState.ModuleDefinitions.Where(item => item.ModuleDefinitionName == ModuleState.ModuleDefinitionName).FirstOrDefault();
            }
        }

        // optional interface properties
        public virtual SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.View; } set { } } // default security

        public virtual string Title { get { return ""; } }

        public virtual string Actions { get { return ""; } }

        public virtual bool UseAdminContainer { get { return true; } }

        // path method

        public string ModulePath()
        {
            return "Modules/" + this.GetType().Namespace + "/";
        }

        // url methods
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

        public string EditUrl(string action)
        {
            return EditUrl(ModuleState.ModuleId, action);
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

        // user feedback methods
        public void AddModuleMessage(string message, MessageType type)
        {
            ModuleInstance.AddModuleMessage(message, type);
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
        public async Task Log(LogLevel level, Exception exception, string message, params object[] args)
        {
            int PageId = PageState.Page.PageId;
            int ModuleId = ModuleState.ModuleId;
            int? UserId = null;
            if (PageState.User != null)
            {
                UserId = PageState.User.UserId;
            }
            string category = this.GetType().AssemblyQualifiedName;
            string feature = Utilities.GetTypeNameLastSegment(category, 1);
            LogFunction function;
            switch (PageState.Action)
            {
                case "Add":
                    function = LogFunction.Create;
                    break;
                case "Edit":
                    function = LogFunction.Update;
                    break;
                case "Delete":
                    function = LogFunction.Delete;
                    break;
                default:
                    function = LogFunction.Read;
                    break;
            }
            if (feature == "Login")
            {
                function = LogFunction.Security;
            }
            await LoggingService.Log(PageId, ModuleId, UserId, category, feature, function, level, exception, message, args);
        }

        public class Logger
        {
            private ModuleBase modulebase;

            public Logger(ModuleBase modulebase)
            {
                this.modulebase = modulebase;
            }

            public async Task LogTrace(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Trace, null, message, args);
            }

            public async Task LogTrace(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Trace, exception, message, args);
            }

            public async Task LogDebug(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Debug, null, message, args);
            }

            public async Task LogDebug(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Debug, exception, message, args);
            }

            public async Task LogInformation(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Information, null, message, args);
            }

            public async Task LogInformation(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Information, exception, message, args);
            }

            public async Task LogWarning(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Warning, null, message, args);
            }

            public async Task LogWarning(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Warning, exception, message, args);
            }

            public async Task LogError(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Error, null, message, args);
            }

            public async Task LogError(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Error, exception, message, args);
            }

            public async Task LogCritical(string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Critical, null, message, args);
            }

            public async Task LogCritical(Exception exception, string message, params object[] args)
            {
                await modulebase.Log(LogLevel.Critical, exception, message, args);
            }
        }
    }
}
