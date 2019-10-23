using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class LogRepository : ILogRepository
    {
        private TenantDBContext db;

        public LogRepository(TenantDBContext context)
        {
            db = context;
        }

        public IEnumerable<Log> GetLogs(int SiteId, string Level, int Rows)
        {
            if (Level == null)
            {
                return db.Log.Where(item => item.SiteId == SiteId).
                    OrderByDescending(item => item.LogDate).Take(Rows);
            }
            else
            {
                return db.Log.Where(item => item.SiteId == SiteId && item.Level == Level)
                    .OrderByDescending(item => item.LogDate).Take(Rows);
            }
        }

        public Log GetLog(int LogId)
        {
            return db.Log.Find(LogId);
        }

        public void AddLog(Log Log)
        {
            db.Log.Add(Log);
            db.SaveChanges();
        }
    }
}
