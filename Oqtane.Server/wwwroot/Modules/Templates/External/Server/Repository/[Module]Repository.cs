using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using System.Threading.Tasks;

namespace [Owner].Module.[Module].Repository
{
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


        public async Task<IEnumerable<Models.[Module]>> Get[Module]sAsync(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return await db.[Module].Where(item => item.ModuleId == ModuleId).ToListAsync();
        }

        public async Task<Models.[Module]> Get[Module]Async(int [Module]Id)
        {
            return await Get[Module]Async([Module]Id, true);
        }

        public async Task<Models.[Module]> Get[Module]Async(int [Module]Id, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return await db.[Module].FindAsync([Module]Id);
            }
            else
            {
                return await db.[Module].AsNoTracking().FirstOrDefaultAsync(item => item.[Module]Id == [Module]Id);
            }
        }

        public async Task<Models.[Module]> Add[Module]Async(Models.[Module] [Module])
        {
            using var db = _factory.CreateDbContext();
            db.[Module].Add([Module]);
            await db.SaveChangesAsync();
            return [Module];
        }

        public async Task<Models.[Module]> Update[Module]Async(Models.[Module] [Module])
        {
            using var db = _factory.CreateDbContext();
            db.Entry([Module]).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return [Module];
        }

        public async Task Delete[Module]Async(int [Module]Id)
        {
            using var db = _factory.CreateDbContext();
            Models.[Module] [Module] = db.[Module].Find([Module]Id);
            db.[Module].Remove([Module]);
            await db.SaveChangesAsync();
        }
    }
}
