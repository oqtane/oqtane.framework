using System.Collections.Generic;
using System.Threading.Tasks;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Services
{
    public interface I[Module]Service 
    {
        Task<List<[Module]>> Get[Module]sAsync(int ModuleId);

        Task<[Module]> Get[Module]Async(int [Module]Id, int ModuleId);

        Task<[Module]> Add[Module]Async([Module] [Module]);

        Task<[Module]> Update[Module]Async([Module] [Module]);

        Task Delete[Module]Async(int [Module]Id, int ModuleId);
    }
}
