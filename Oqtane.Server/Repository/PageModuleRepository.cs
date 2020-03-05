using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageModuleRepository : IPageModuleRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public PageModuleRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            _db = context;
            _permissions = Permissions;
        }

        public IEnumerable<PageModule> GetPageModules(int SiteId)
        {
            IEnumerable<PageModule> pagemodules = _db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.Module.SiteId == SiteId);
            if (pagemodules != null && pagemodules.Any())
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    pagemodule.Module.Permissions = _permissions.EncodePermissions(pagemodule.ModuleId, permissions);
                }
            }
            return pagemodules;
        }

        public IEnumerable<PageModule> GetPageModules(int PageId, string Pane)
        {
            IEnumerable<PageModule> pagemodules = _db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.PageId == PageId);
            if (Pane != "" && pagemodules != null && pagemodules.Any())
            {
                pagemodules = pagemodules.Where(item => item.Pane == Pane);
            }
            if (pagemodules != null && pagemodules.Any())
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    pagemodule.Module.Permissions = _permissions.EncodePermissions(pagemodule.ModuleId, permissions);
                }
            }
            return pagemodules;
        }

        public PageModule AddPageModule(PageModule PageModule)
        {
            _db.PageModule.Add(PageModule);
            _db.SaveChanges();
            return PageModule;
        }

        public PageModule UpdatePageModule(PageModule PageModule)
        {
            _db.Entry(PageModule).State = EntityState.Modified;
            _db.SaveChanges();
            return PageModule;
        }

        public PageModule GetPageModule(int PageModuleId)
        {
            PageModule pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageModuleId == PageModuleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Module", pagemodule.ModuleId).ToList();
                pagemodule.Module.Permissions = _permissions.EncodePermissions(pagemodule.ModuleId, permissions);
            }
            return pagemodule;
        }

        public PageModule GetPageModule(int PageId, int ModuleId)
        {
            PageModule pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageId == PageId && item.ModuleId == ModuleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Module", pagemodule.ModuleId).ToList();
                pagemodule.Module.Permissions = _permissions.EncodePermissions(pagemodule.ModuleId, permissions);
            }
            return pagemodule;
        }

        public void DeletePageModule(int PageModuleId)
        {
            PageModule PageModule = _db.PageModule.Find(PageModuleId);
            _db.PageModule.Remove(PageModule);
            _db.SaveChanges();
        }
    }
}
