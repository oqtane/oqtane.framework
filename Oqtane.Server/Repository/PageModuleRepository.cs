using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageModuleRepository : IPageModuleRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;

        public PageModuleRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
        }

        public IEnumerable<PageModule> GetPageModules()
        {
            return db.PageModule;
        }
        public IEnumerable<PageModule> GetPageModules(int SiteId)
        {
            IEnumerable<PageModule> pagemodules = db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.Module.SiteId == SiteId);
            if (pagemodules != null && pagemodules.Any())
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    pagemodule.Module.Permissions = Permissions.EncodePermissions(pagemodule.ModuleId, permissions);
                }
            }
            return pagemodules;
        }

        public PageModule AddPageModule(PageModule PageModule)
        {
            db.PageModule.Add(PageModule);
            db.SaveChanges();
            return PageModule;
        }

        public PageModule UpdatePageModule(PageModule PageModule)
        {
            db.Entry(PageModule).State = EntityState.Modified;
            db.SaveChanges();
            return PageModule;
        }

        public PageModule GetPageModule(int PageModuleId)
        {
            PageModule pagemodule = db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageModuleId == PageModuleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Module", pagemodule.ModuleId);
                pagemodule.Module.Permissions = Permissions.EncodePermissions(pagemodule.ModuleId, permissions);
            }
            return pagemodule;
        }

        public PageModule GetPageModule(int PageId, int ModuleId)
        {
            PageModule pagemodule = db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageId == PageId && item.ModuleId == ModuleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Module", pagemodule.ModuleId);
                pagemodule.Module.Permissions = Permissions.EncodePermissions(pagemodule.ModuleId, permissions);
            }
            return pagemodule;
        }

        public void DeletePageModule(int PageModuleId)
        {
            PageModule PageModule = db.PageModule.Find(PageModuleId);
            db.PageModule.Remove(PageModule);
            db.SaveChanges();
        }
    }
}
