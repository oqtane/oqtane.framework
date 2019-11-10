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

        public PageRepository(TenantDBContext context, IPermissionRepository Permissions, IPageModuleRepository PageModules)
        {
            db = context;
            this.Permissions = Permissions;
            this.PageModules = PageModules;
        }

        public IEnumerable<Page> GetPages()
        {
            return db.Page.ToList();
        }

        public IEnumerable<Page> GetPages(int SiteId)
        {
            IEnumerable<Permission> permissions = Permissions.GetPermissions(SiteId, "Page").ToList();
            IEnumerable<Page> pages = db.Page.Where(item => item.SiteId == SiteId && item.UserId == null);
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

        public Page GetPage(int PageId, int UserId)
        {
            Page page = db.Page.Find(PageId);
            if (page != null)
            {
                Page personalized = db.Page.Where(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == UserId).FirstOrDefault();
                if (personalized != null)
                {
                    page = personalized;
                }
                if (page != null)
                {
                    IEnumerable<Permission> permissions = Permissions.GetPermissions("Page", page.PageId);
                    page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
                }
            }
            return page;
        }

        public void DeletePage(int PageId)
        {
            Page Page = db.Page.Find(PageId);
            Permissions.DeletePermissions(Page.SiteId, "Page", PageId);
            IEnumerable<PageModule> pageModules = db.PageModule.Where(item => item.PageId == PageId).ToList();
            foreach (var pageModule in pageModules)
            {
                PageModules.DeletePageModule(pageModule.PageModuleId);
            }
            db.Page.Remove(Page);
            db.SaveChanges();
        }
    }
}
