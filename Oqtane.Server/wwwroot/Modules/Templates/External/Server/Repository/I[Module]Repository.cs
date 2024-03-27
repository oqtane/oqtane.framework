using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].Module.[Module].Repository
{
    public interface I[Module]Repository
    {
        IEnumerable<Models.[Module]> Get[Module]s(int ModuleId);
        Models.[Module] Get[Module](int [Module]Id);
        Models.[Module] Get[Module](int [Module]Id, bool tracking);
        Models.[Module] Add[Module](Models.[Module] [Module]);
        Models.[Module] Update[Module](Models.[Module] [Module]);
        void Delete[Module](int [Module]Id);

        Task<IEnumerable<Models.[Module]>> Get[Module]sAsync(int ModuleId);
        Task<Models.[Module]> Get[Module]Async(int [Module]Id);
        Task<Models.[Module]> Get[Module]Async(int [Module]Id, bool tracking);
        Task<Models.[Module]> Add[Module]Async(Models.[Module] [Module]);
        Task<Models.[Module]> Update[Module]Async(Models.[Module] [Module]);
        Task Delete[Module]Async(int [Module]Id);
    }
}
