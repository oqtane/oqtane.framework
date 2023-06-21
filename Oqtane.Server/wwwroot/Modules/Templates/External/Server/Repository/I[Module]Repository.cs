using System.Collections.Generic;
using [Owner].Module.[Module].Models;

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
    }
}
