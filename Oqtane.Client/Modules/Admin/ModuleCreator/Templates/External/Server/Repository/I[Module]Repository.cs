using System.Collections.Generic;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Repository
{
    public interface I[Module]Repository
    {
        IEnumerable<[Module]> Get[Module]s(int ModuleId);
        [Module] Get[Module](int [Module]Id);
        [Module] Add[Module]([Module] [Module]);
        [Module] Update[Module]([Module] [Module]);
        void Delete[Module](int [Module]Id);
    }
}
