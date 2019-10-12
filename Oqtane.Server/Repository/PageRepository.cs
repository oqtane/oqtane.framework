using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageRepository : IPageRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;
        private readonly IPageModuleRepository PageModules;
        private readonly IModuleRepository ModuleRepository;

        public PageRepository(TenantDBContext context, IPermissionRepository Permissions, IPageModuleRepository PageModules, IModuleRepository ModuleRepository)
        {
            db = context;
            this.Permissions = Permissions;
            this.PageModules = PageModules;
            this.ModuleRepository = ModuleRepository;
        }

        public IEnumerable<Page> GetPages()
        {
            return db.Page.ToList();
        }

        public IEnumerable<Page> GetPages(int SiteId)
        {
            IEnumerable<Permission> permissions = Permissions.GetPermissions(SiteId, "Page").ToList();
            IEnumerable<Page> pages = db.Page.Where(item => item.SiteId == SiteId);
            foreach(Page page in pages)
            {
                page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
            }
            return pages;
        }

        public Page AddPage(Page Page)
        {
            db.Page.Add(Page);
            db.SaveChanges();
            Permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
            return Page;
        }

        public Page UpdatePage(Page Page)
        {
            db.Entry(Page).State = EntityState.Modified;
            db.SaveChanges();
            Permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
            return Page;
        }

        public Page GetPage(int PageId)
        {
            Page page = db.Page.Find(PageId);
            if (page != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Page", page.PageId);
                page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
            }
            return page;
        }

        public void DeletePage(int PageId)
        {
            Page Page = db.Page.Find(PageId);

            IEnumerable<PageModule> pageModules = db.PageModule.Where(item => item.PageId == PageId).ToList();
            foreach (var pageModule in pageModules)
            {
                PageModules.DeletePageModule(pageModule.PageModuleId);

                IEnumerable<Module> modules = db.Module.Where(item => item.ModuleId == pageModule.ModuleId).ToList();
                foreach (var module in modules)
                {
                    ModuleRepository.DeleteModule(module.ModuleId);
                }
            }

            Permissions.DeletePermissions(Page.SiteId, "Page", PageId);

            db.Page.Remove(Page);
            db.SaveChanges();
        }
    }
}
