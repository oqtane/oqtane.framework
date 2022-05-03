using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class LogService : ServiceBase, ILogService
    {
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public LogService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http, siteState)
        {
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl => CreateApiUrl("Log");

        public async Task<List<Log>> GetLogsAsync(int siteId, string level, string function, int rows)
        {
            return await GetJsonAsync<List<Log>>($"{Apiurl}?siteid={siteId}&level={level}&function={function}&rows={rows}");
        }

        public async Task<Log> GetLogAsync(int logId)
        {
            return await GetJsonAsync<Log>($"{Apiurl}/{logId}");
        }

        public async Task Log(int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args)
        {
            await Log(null, pageId, moduleId, userId, category, feature, function, level, exception, message, args);
        }

        public async Task Log(Alias alias, int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args)
        {
            Log log = new Log();
            if (alias == null)
            {
                log.SiteId = _siteState.Alias.SiteId;
            }
            else
            {
                log.SiteId = alias.SiteId;
            }
            log.PageId = pageId;
            log.ModuleId = moduleId;
            log.UserId = userId;
            log.Url = _navigationManager.Uri;
            log.Category = category;
            log.Feature = feature;
            log.Function = Enum.GetName(typeof(LogFunction), function);
            log.Level = Enum.GetName(typeof(LogLevel), level);
            if (exception != null)
            {
                log.Exception = exception.ToString();
            }
            log.Message = message;
            log.MessageTemplate = "";
            log.Properties = JsonSerializer.Serialize(args);
            await PostJsonAsync(Apiurl, log);
        }
    }
}
