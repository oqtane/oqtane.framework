using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using [Owner].Module.[Module].Models;

namespace [Owner].Module.[Module].Repository
{
    public class [Module]Repository : I[Module]Repository, ITransientService
    {
        private readonly IDbContextFactory<[Module]Context> _factory;
        private readonly [Module]Context _queryContext;

        public [Module]Repository(IDbContextFactory<[Module]Context> factory)
        {
            _factory = factory;
            _queryContext = _factory.CreateDbContext();
        }

        public IEnumerable<Models.[Module]> Get[Module]s(int ModuleId)
        {
            return _queryContext.[Module].Where(item => item.ModuleId == ModuleId);
        }

        public Models.[Module] Get[Module](int [Module]Id)
        {
            return Get[Module]([Module]Id, true);
        }

        public Models.[Module] Get[Module](int [Module]Id, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.[Module].Find([Module]Id);
            }
            else
            {
                return db.[Module].AsNoTracking().FirstOrDefault(item => item.[Module]Id == [Module]Id);
            }
        }

        public Models.[Module] Add[Module](Models.[Module] [Module])
        {
            using var db = _factory.CreateDbContext();
            db.[Module].Add([Module]);
            db.SaveChanges();
            return [Module];
        }

        public Models.[Module] Update[Module](Models.[Module] [Module])
        {
            using var db = _factory.CreateDbContext();
            db.Entry([Module]).State = EntityState.Modified;
            db.SaveChanges();
            return [Module];
        }

        public void Delete[Module](int [Module]Id)
        {
            using var db = _factory.CreateDbContext();
            Models.[Module] [Module] = db.[Module].Find([Module]Id);
            db.[Module].Remove([Module]);
            db.SaveChanges();
        }
    }
}
