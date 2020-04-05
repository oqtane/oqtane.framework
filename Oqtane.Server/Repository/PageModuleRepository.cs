using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageModuleRepository : IPageModuleRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public PageModuleRepository(TenantDBContext context, IPermissionRepository permissions)
        {
            _db = context;
            _permissions = permissions;
        }

        public IEnumerable<PageModule> GetPageModules(int siteId)
        {
            IEnumerable<PageModule> pagemodules = _db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.Module.SiteId == siteId);
            if (pagemodules != null && pagemodules.Any())
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    pagemodule.Module.Permissions = _permissions.EncodePermissions(permissions.Where(item => item.EntityId == pagemodule.ModuleId));
                }
            }
            return pagemodules;
        }

        public IEnumerable<PageModule> GetPageModules(int pageId, string pane)
        {
            IEnumerable<PageModule> pagemodules = _db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.PageId == pageId);
            if (pane != "" && pagemodules != null && pagemodules.Any())
            {
                pagemodules = pagemodules.Where(item => item.Pane == pane);
            }
            if (pagemodules != null && pagemodules.Any())
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    pagemodule.Module.Permissions = _permissions.EncodePermissions(permissions.Where(item => item.EntityId == pagemodule.ModuleId));
                }
            }
            return pagemodules;
        }

        public PageModule AddPageModule(PageModule pageModule)
        {
            _db.PageModule.Add(pageModule);
            _db.SaveChanges();
            return pageModule;
        }

        public PageModule UpdatePageModule(PageModule pageModule)
        {
            _db.Entry(pageModule).State = EntityState.Modified;
            _db.SaveChanges();
            return pageModule;
        }

        public PageModule GetPageModule(int pageModuleId)
        {
            PageModule pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageModuleId == pageModuleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Module", pagemodule.ModuleId).ToList();
                pagemodule.Module.Permissions = _permissions.EncodePermissions(permissions);
            }
            return pagemodule;
        }

        public PageModule GetPageModule(int pageId, int moduleId)
        {
            PageModule pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageId == pageId && item.ModuleId == moduleId);
            if (pagemodule != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Module", pagemodule.ModuleId).ToList();
                pagemodule.Module.Permissions = _permissions.EncodePermissions(permissions);
            }
            return pagemodule;
        }

        public void DeletePageModule(int pageModuleId)
        {
            PageModule pageModule = _db.PageModule.Find(pageModuleId);
            _db.PageModule.Remove(pageModule);
            _db.SaveChanges();
        }
    }
}
