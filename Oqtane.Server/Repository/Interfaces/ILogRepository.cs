using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ILogRepository
    {
        IEnumerable<Log> GetLogs(int siteId, string level, string function, int rows);
        Log GetLog(int logId);
        void AddLog(Log log);
        int DeleteLogs(int siteId, int age);
    }
}
