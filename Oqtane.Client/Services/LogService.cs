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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public LogService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Log"); }
        }

        public async Task<List<Log>> GetLogsAsync(int SiteId, string Level, string Function, int Rows)
        {
            return await http.GetJsonAsync<List<Log>>(apiurl + "?siteid=" + SiteId.ToString() + "&level=" + Level + "&function=" + Function + "&rows=" + Rows.ToString());
        }

        public async Task<Log> GetLogAsync(int LogId)
        {
            return await http.GetJsonAsync<Log>(apiurl + "/" + LogId.ToString());
        }

        public async Task Log(int? PageId, int? ModuleId, int? UserId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args)
        {
            Log log = new Log();
            log.SiteId = sitestate.Alias.SiteId;
            log.PageId = PageId;
            log.ModuleId = ModuleId;
            log.UserId = UserId;
            log.Url = NavigationManager.Uri;
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
            await http.PostJsonAsync(apiurl, log);
        }
    }
}
