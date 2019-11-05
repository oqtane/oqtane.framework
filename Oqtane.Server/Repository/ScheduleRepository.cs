using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Oqtane.Repository
{
    public class ScheduleRepository : IScheduleRepository
    {
        private MasterDBContext db;

        public ScheduleRepository(MasterDBContext context)
        {
            db = context;
        }

        public IEnumerable<Schedule> GetSchedules()
        {
            return db.Schedule.ToList();
        }

        public Schedule AddSchedule(Schedule Schedule)
        {
            db.Schedule.Add(Schedule);
            db.SaveChanges();
            return Schedule;
        }

        public Schedule UpdateSchedule(Schedule Schedule)
        {
            db.Entry(Schedule).State = EntityState.Modified;
            db.SaveChanges();
            return Schedule;
        }

        public Schedule GetSchedule(int ScheduleId)
        {
            return db.Schedule.Find(ScheduleId);
        }

        public void DeleteSchedule(int ScheduleId)
        {
            Schedule schedule = db.Schedule.Find(ScheduleId);
            db.Schedule.Remove(schedule);
            db.SaveChanges();
        }
    }
}
