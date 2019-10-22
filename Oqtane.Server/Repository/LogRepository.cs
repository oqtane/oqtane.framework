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

        public void AddLog(Log Log)
        {
            db.Log.Add(Log);
            db.SaveChanges();
        }

        public IEnumerable<Log> GetLogs(int SiteId)
        {
            return db.Log.Where(item => item.SiteId == SiteId).OrderByDescending(item=> item.LogDate).Take(50);
        }
    }
}
