using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

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

    public class [Module]Repository : I[Module]Repository, ITransientService
    {
        private readonly IDbContextFactory<[Module]Context> _factory;

        public [Module]Repository(IDbContextFactory<[Module]Context> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.[Module]> Get[Module]s(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.[Module].Where(item => item.ModuleId == ModuleId).ToList();
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
