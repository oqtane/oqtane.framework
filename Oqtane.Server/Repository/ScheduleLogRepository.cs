using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Oqtane.Repository
{
    public class ScheduleLogRepository : IScheduleLogRepository
    {
        private MasterDBContext db;

        public ScheduleLogRepository(MasterDBContext context)
        {
            db = context;
        }

        public IEnumerable<ScheduleLog> GetScheduleLogs()
        {
            return db.ScheduleLog.ToList();
        }

        public ScheduleLog AddScheduleLog(ScheduleLog ScheduleLog)
        {
            db.ScheduleLog.Add(ScheduleLog);
            db.SaveChanges();
            return ScheduleLog;
        }

        public ScheduleLog UpdateScheduleLog(ScheduleLog ScheduleLog)
        {
            db.Entry(ScheduleLog).State = EntityState.Modified;
            db.SaveChanges();
            return ScheduleLog;
        }

        public ScheduleLog GetScheduleLog(int ScheduleLogId)
        {
            return db.ScheduleLog.Find(ScheduleLogId);
        }

        public void DeleteScheduleLog(int ScheduleLogId)
        {
            ScheduleLog schedulelog = db.ScheduleLog.Find(ScheduleLogId);
            db.ScheduleLog.Remove(schedulelog);
            db.SaveChanges();
        }
    }
}
