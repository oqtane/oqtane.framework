using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Repository
{
    public interface ILogRepository
    {
        IEnumerable<Log> GetLogs(int SiteId, string Level, string Function, int Rows);
        Log GetLog(int LogId);
        void AddLog(Log Log);
    }
}
