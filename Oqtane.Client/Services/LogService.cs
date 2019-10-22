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

        public async Task<List<Log>> GetLogsAsync(int SiteId)
        {
            return await http.GetJsonAsync<List<Log>>(apiurl + "?siteid=" + SiteId.ToString());
        }

        public async Task Log(int? PageId, int? ModuleId, int? UserId, string category, LogLevel level, Exception exception, string message, params object[] args)
        {
            Log log = new Log();
            log.SiteId = sitestate.Alias.SiteId;
            log.PageId = PageId;
            log.ModuleId = ModuleId;
            log.UserId = UserId;
            log.Url = NavigationManager.Uri;
            log.Category = category;
            log.Level = Enum.GetName(typeof(LogLevel), level);
            if (exception != null)
            {
                log.Exception = JsonSerializer.Serialize(exception);
            }
            log.Message = message;
            log.MessageTemplate = "";
            log.Properties = JsonSerializer.Serialize(args);
            await http.PostJsonAsync(apiurl, log);
        }
    }
}
