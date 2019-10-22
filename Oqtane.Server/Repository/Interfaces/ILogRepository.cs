using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Repository
{
    public interface ILogRepository
    {
        void AddLog(Log Log);
        IEnumerable<Log> GetLogs(int SiteId);
    }
}
