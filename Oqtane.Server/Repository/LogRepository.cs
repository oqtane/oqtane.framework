using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class LogRepository : ILogRepository
    {
        private TenantDBContext _db;

        public LogRepository(TenantDBContext context)
        {
            _db = context;
        }

        public IEnumerable<Log> GetLogs(int siteId, string level, string function, int rows)
        {
            if (level == null)
            {
                if (function == null)
                {
                    return _db.Log.Where(item => item.SiteId == siteId).
                        OrderByDescending(item => item.LogDate).Take(rows);
                }

                return _db.Log.Where(item => item.SiteId == siteId && item.Function == function).
                    OrderByDescending(item => item.LogDate).Take(rows);
            }

            if (function == null)
            {
                return _db.Log.Where(item => item.SiteId == siteId && item.Level == level)
                    .OrderByDescending(item => item.LogDate).Take(rows);
            }

            return _db.Log.Where(item => item.SiteId == siteId && item.Level == level && item.Function == function)
                .OrderByDescending(item => item.LogDate).Take(rows);
        }

        public Log GetLog(int logId)
        {
            return _db.Log.Find(logId);
        }

        public void AddLog(Log log)
        {
            _db.Log.Add(log);
            _db.SaveChanges();
        }
    }
}
