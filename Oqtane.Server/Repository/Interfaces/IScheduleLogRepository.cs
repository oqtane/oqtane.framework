using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IScheduleLogRepository
    {
        IEnumerable<ScheduleLog> GetScheduleLogs();
        ScheduleLog AddScheduleLog(ScheduleLog ScheduleLog);
        ScheduleLog UpdateScheduleLog(ScheduleLog ScheduleLog);
        ScheduleLog GetScheduleLog(int ScheduleLogId);
        void DeleteScheduleLog(int ScheduleLogId);
    }
}
