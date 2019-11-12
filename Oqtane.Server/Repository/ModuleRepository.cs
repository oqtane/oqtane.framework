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
            return db.Module;
        }

        public Module AddModule(Module Module)
        {
            db.Module.Add(Module);
            db.SaveChanges();
            Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Module UpdateModule(Module Module)
        {
            db.Entry(Module).State = EntityState.Modified;
            db.SaveChanges();
            Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Module GetModule(int ModuleId)
        {
            Module module = db.Module.Find(ModuleId);
            if (module != null)
            {
                List<Permission> permissions = Permissions.GetPermissions("Module", module.ModuleId).ToList();
                module.Permissions = Permissions.EncodePermissions(module.ModuleId, permissions);
            }
            return module;
        }

        public void DeleteModule(int ModuleId)
        {
            Module Module = db.Module.Find(ModuleId);
            Permissions.DeletePermissions(Module.SiteId, "Module", ModuleId);
            db.Module.Remove(Module);
            db.SaveChanges();
        }
    }
}
