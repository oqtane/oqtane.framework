using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IScheduleRepository
    {
        IEnumerable<Schedule> GetSchedules();
        Schedule AddSchedule(Schedule Schedule);
        Schedule UpdateSchedule(Schedule Schedule);
        Schedule GetSchedule(int ScheduleId);
        void DeleteSchedule(int ScheduleId);
    }
}
