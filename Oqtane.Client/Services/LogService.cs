using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class LogService : ServiceBase, ILogService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public LogService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Log"); }
        }

        public async Task<List<Log>> GetLogsAsync(int SiteId, string Level, string Function, int Rows)
        {
            return await _http.GetJsonAsync<List<Log>>(apiurl + "?siteid=" + SiteId.ToString() + "&level=" + Level + "&function=" + Function + "&rows=" + Rows.ToString());
        }

        public async Task<Log> GetLogAsync(int LogId)
        {
            return await _http.GetJsonAsync<Log>(apiurl + "/" + LogId.ToString());
        }

        public async Task Log(int? PageId, int? ModuleId, int? UserId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args)
        {
            await Log(null, PageId, ModuleId, UserId, category, feature, function, level, exception, message, args);
        }

        public async Task Log(Alias Alias, int? PageId, int? ModuleId, int? UserId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args)
        {
            Log log = new Log();
            if (Alias == null)
            {
                log.SiteId = _siteState.Alias.SiteId;
            }
            else
            {
                log.SiteId = Alias.SiteId;
            }
            log.PageId = PageId;
            log.ModuleId = ModuleId;
            log.UserId = UserId;
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
            await _http.PostJsonAsync(CreateCrossTenantUrl(apiurl, Alias), log);
        }
    }
}
