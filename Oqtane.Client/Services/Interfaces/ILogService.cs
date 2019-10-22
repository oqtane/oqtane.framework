using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ILogService
    {
        Task<List<Log>> GetLogsAsync(int SiteId);
        Task Log(int? PageId, int? ModuleId, int? UserId, string component, LogLevel level, Exception exception, string message, params object[] args);
    }
}
