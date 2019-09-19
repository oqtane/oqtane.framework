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

        public PageRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
        }

        public IEnumerable<Page> GetPages(bool IgnoreFilter = false)
        {
            var query = db.Page;

            if (IgnoreFilter)
            {
                query.IgnoreQueryFilters();
            }

            return query.ToList();
        }

        public IEnumerable<Page> GetPages(int SiteId, bool IgnoreFilter  = false)
        {
            IEnumerable<Permission> permissions = Permissions.GetPermissions(SiteId, "Page").ToList();
            IQueryable<Page> query = db.Page.Where(item => item.SiteId == SiteId);

            if (IgnoreFilter)
            {
                query = query.IgnoreQueryFilters();
            }

            IEnumerable<Page> pages = query;

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
            Permissions.UpdatePermissions(Page.SiteId, "Page", PageId, "");
            db.Page.Remove(Page);
            db.SaveChanges();
        }
    }
}
