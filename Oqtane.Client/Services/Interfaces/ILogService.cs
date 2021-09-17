using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Enums;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and store <see cref="Log"/> entries
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Returns a list of log entires for the given params
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="level"></param>
        /// <param name="function"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        Task<List<Log>> GetLogsAsync(int siteId, string level, string function, int rows);

        /// <summary>
        /// Returns a specific log entry for the given id
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        Task<Log> GetLogAsync(int logId);

        /// <summary>
        /// Creates a new log entry
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="moduleId"></param>
        /// <param name="userId"></param>
        /// <param name="category"></param>
        /// <param name="feature"></param>
        /// <param name="function"></param>
        /// <param name="level"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Log(int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args);

        /// <summary>
        /// Creates a new log entry
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="pageId"></param>
        /// <param name="moduleId"></param>
        /// <param name="userId"></param>
        /// <param name="category"></param>
        /// <param name="feature"></param>
        /// <param name="function"></param>
        /// <param name="level"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Log(Alias alias, int? pageId, int? moduleId, int? userId, string category, string feature, LogFunction function, LogLevel level, Exception exception, string message, params object[] args);
    }
}
