using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class ModuleRepository : IModuleRepository
    {
        private TenantContext db;

        public ModuleRepository(TenantContext context)
        {
            db = context;
        }

        public IEnumerable<Module> GetModules()
        {
            try
            {
                return db.Module.ToList();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Module> GetModules(int SiteId, string ModuleDefinitionName)
        {
            try
            {
                return db.Module
                    .Where(item => item.SiteId == SiteId)
                    .Where(item => item.ModuleDefinitionName == ModuleDefinitionName)
                    .ToList();
            }
            catch
            {
                throw;
            }
        }

        public void AddModule(Module Module)
        {
            try
            {
                db.Module.Add(Module);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateModule(Module Module)
        {
            try
            {
                db.Entry(Module).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Module GetModule(int ModuleId)
        {
            try
            {
                Module Module = db.Module.Find(ModuleId);
                return Module;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteModule(int ModuleId)
        {
            try
            {
                Module Module = db.Module.Find(ModuleId);
                db.Module.Remove(Module);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
