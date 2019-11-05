using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IScheduleService
    {
        Task<List<Schedule>> GetSchedulesAsync();

        Task<Schedule> GetScheduleAsync(int ScheduleId);

        Task<Schedule> AddScheduleAsync(Schedule Schedule);

        Task<Schedule> UpdateScheduleAsync(Schedule Schedule);

        Task DeleteScheduleAsync(int ScheduleId);
    }
}
