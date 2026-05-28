using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].Module.[Module].Services
{
    public interface I[Module]Service 
    {
        Task<List<Models.[Module]>> Get[Module]sAsync(int ModuleId);

        Task<Models.[Module]> Get[Module]Async(int [Module]Id, int ModuleId);

        Task<Models.[Module]> Add[Module]Async(Models.[Module] [Module]);

        Task<Models.[Module]> Update[Module]Async(Models.[Module] [Module]);

        Task Delete[Module]Async(int [Module]Id, int ModuleId);
    }
}
