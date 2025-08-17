using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace Oqtane.Application.Repository
{
    public interface IMyModuleRepository
    {
        IEnumerable<Models.MyModule> GetMyModules(int ModuleId);
        Models.MyModule GetMyModule(int MyModuleId);
        Models.MyModule GetMyModule(int MyModuleId, bool tracking);
        Models.MyModule AddMyModule(Models.MyModule MyModule);
        Models.MyModule UpdateMyModule(Models.MyModule MyModule);
        void DeleteMyModule(int MyModuleId);
    }

    public class MyModuleRepository : IMyModuleRepository, ITransientService
    {
        private readonly IDbContextFactory<Context> _factory;

        public MyModuleRepository(IDbContextFactory<Context> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.MyModule> GetMyModules(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.MyModule.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.MyModule GetMyModule(int MyModuleId)
        {
            return GetMyModule(MyModuleId, true);
        }

        public Models.MyModule GetMyModule(int MyModuleId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.MyModule.Find(MyModuleId);
            }
            else
            {
                return db.MyModule.AsNoTracking().FirstOrDefault(item => item.MyModuleId == MyModuleId);
            }
        }

        public Models.MyModule AddMyModule(Models.MyModule MyModule)
        {
            using var db = _factory.CreateDbContext();
            db.MyModule.Add(MyModule);
            db.SaveChanges();
            return MyModule;
        }

        public Models.MyModule UpdateMyModule(Models.MyModule MyModule)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(MyModule).State = EntityState.Modified;
            db.SaveChanges();
            return MyModule;
        }

        public void DeleteMyModule(int MyModuleId)
        {
            using var db = _factory.CreateDbContext();
            Models.MyModule MyModule = db.MyModule.Find(MyModuleId);
            db.MyModule.Remove(MyModule);
            db.SaveChanges();
        }
    }
}
