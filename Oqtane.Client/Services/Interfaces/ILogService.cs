using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Enums;

namespace Oqtane.Services
{
    public interface ILogService
    {
        Task<List<Log>> GetLogsAsync(int siteId, string level, string function, int rows);
        Task<Log> GetLogAsync(int logId);
        Task Log(int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args);
        Task Log(Alias alias, int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args);
    }
}
