using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IScheduleLogService
    {
        Task<List<ScheduleLog>> GetScheduleLogsAsync();

        Task<ScheduleLog> GetScheduleLogAsync(int ScheduleLogId);

        Task<ScheduleLog> AddScheduleLogAsync(ScheduleLog ScheduleLog);

        Task<ScheduleLog> UpdateScheduleLogAsync(ScheduleLog ScheduleLog);

        Task DeleteScheduleLogAsync(int ScheduleLogId);
    }
}
