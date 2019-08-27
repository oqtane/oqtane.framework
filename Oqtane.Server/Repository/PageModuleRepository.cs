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
            try
            {
                return db.PageModule.ToList();
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<PageModule> GetPageModules(int PageId)
        {
            try
            {
                List<PageModule> pagemodules = db.PageModule.Where(item => item.PageId == PageId)
                    .Include(item => item.Module) // eager load modules
                    .ToList();
                if (pagemodules != null && pagemodules.Any())
                {
                    List<Permission> permissions = Permissions.GetPermissions(pagemodules.FirstOrDefault().Module.SiteId, "Module").ToList();
                    foreach (PageModule pagemodule in pagemodules)
                    {
                        pagemodule.Module.Permissions = Permissions.EncodePermissions(pagemodule.ModuleId, permissions);
                    }
                }
                return pagemodules;
            }
            catch
            {
                throw;
            }
        }

        public PageModule AddPageModule(PageModule PageModule)
        {
            try
            {
                db.PageModule.Add(PageModule);
                db.SaveChanges();
                return PageModule;
            }
            catch
            {
                throw;
            }
        }

        public PageModule UpdatePageModule(PageModule PageModule)
        {
            try
            {
                db.Entry(PageModule).State = EntityState.Modified;
                db.SaveChanges();
                return PageModule;
            }
            catch
            {
                throw;
            }
        }

        public PageModule GetPageModule(int PageModuleId)
        {
            try
            {
                PageModule pagemodule = db.PageModule.Include(item => item.Module) // eager load modules
                    .SingleOrDefault(item => item.PageModuleId == PageModuleId);
                if (pagemodule != null)
                {
                    List<Permission> permissions = Permissions.GetPermissions("Module", pagemodule.ModuleId).ToList();
                    pagemodule.Module.Permissions = Permissions.EncodePermissions(pagemodule.ModuleId, permissions);
                }
                return pagemodule;
            }
            catch
            {
                throw;
            }
        }

        public void DeletePageModule(int PageModuleId)
        {
            try
            {
                PageModule PageModule = db.PageModule.Find(PageModuleId);
                db.PageModule.Remove(PageModule);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
