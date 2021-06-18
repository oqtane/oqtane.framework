using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using [Owner].[Module].Models;

namespace [Owner].[Module].Repository
{
    public class [Module]Repository : I[Module]Repository, IService
    {
        private readonly [Module]Context _db;

        public [Module]Repository([Module]Context context)
        {
            _db = context;
        }

        public IEnumerable<Models.[Module]> Get[Module]s(int ModuleId)
        {
            return _db.[Module].Where(item => item.ModuleId == ModuleId);
        }

        public Models.[Module] Get[Module](int [Module]Id)
        {
            return Get[Module]([Module]Id, true);
        }

        public Models.[Module] Get[Module](int [Module]Id, bool tracking)
        {
            if (tracking)
            {
                return _db.[Module].Find([Module]Id);
            }
            else
            {
                return _db.[Module].AsNoTracking().FirstOrDefault(item => item.[Module]Id == [Module]Id);
            }
        }

        public Models.[Module] Add[Module](Models.[Module] [Module])
        {
            _db.[Module].Add([Module]);
            _db.SaveChanges();
            return [Module];
        }

        public Models.[Module] Update[Module](Models.[Module] [Module])
        {
            _db.Entry([Module]).State = EntityState.Modified;
            _db.SaveChanges();
            return [Module];
        }

        public void Delete[Module](int [Module]Id)
        {
            Models.[Module] [Module] = _db.[Module].Find([Module]Id);
            _db.[Module].Remove([Module]);
            _db.SaveChanges();
        }
    }
}
