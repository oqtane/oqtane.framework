using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Repository
{
    public class [Module]Repository : I[Module]Repository, IService
    {
        private readonly [Module]Context _db;

        public [Module]Repository([Module]Context context)
        {
            _db = context;
        }

        public IEnumerable<[Module]> Get[Module]s(int ModuleId)
        {
            return _db.[Module].Where(item => item.ModuleId == ModuleId);
        }

        public [Module] Get[Module](int [Module]Id)
        {
            return _db.[Module].Find([Module]Id);
        }

        public [Module] Add[Module]([Module] [Module])
        {
            _db.[Module].Add([Module]);
            _db.SaveChanges();
            return [Module];
        }

        public [Module] Update[Module]([Module] [Module])
        {
            _db.Entry([Module]).State = EntityState.Modified;
            _db.SaveChanges();
            return [Module];
        }

        public void Delete[Module](int [Module]Id)
        {
            [Module] [Module] = _db.[Module].Find([Module]Id);
            _db.[Module].Remove([Module]);
            _db.SaveChanges();
        }
    }
}
