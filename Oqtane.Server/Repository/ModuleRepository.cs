using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class ModuleRepository : IModuleRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;

        public ModuleRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
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
                List<Permission> permissions = Permissions.GetPermissions(SiteId, "Module").ToList();
                List<Module> modules = db.Module
                    .Where(item => item.SiteId == SiteId)
                    .Where(item => item.ModuleDefinitionName == ModuleDefinitionName)
                    .ToList();
                foreach (Module module in modules)
                {
                    module.Permissions = Permissions.EncodePermissions(module.ModuleId, permissions);
                }
                return modules;
            }
            catch
            {
                throw;
            }
        }

        public Module AddModule(Module Module)
        {
            try
            {
                db.Module.Add(Module);
                db.SaveChanges();
                Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
                return Module;
            }
            catch
            {
                throw;
            }
        }

        public Module UpdateModule(Module Module)
        {
            try
            {
                db.Entry(Module).State = EntityState.Modified;
                db.SaveChanges();
                Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
                return Module;
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
                Module module = db.Module.Find(ModuleId);
                if (module != null)
                {
                    List<Permission> permissions = Permissions.GetPermissions("Module", module.ModuleId).ToList();
                    module.Permissions = Permissions.EncodePermissions(module.ModuleId, permissions);
                }
                return module;
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
                Permissions.UpdatePermissions(Module.SiteId, "Module", ModuleId, "");
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
