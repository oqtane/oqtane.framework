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

        public IEnumerable<Page> GetPages()
        {
            try
            {
                return db.Page.ToList();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Page> GetPages(int SiteId)
        {
            try
            {
                List<Permission> permissions = Permissions.GetPermissions(SiteId, "Page").ToList();
                List<Page> pages = db.Page.Where(item => item.SiteId == SiteId).ToList();
                foreach(Page page in pages)
                {
                    page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
                }
                return pages;
            }
            catch
            {
                throw;
            }
        }

        public Page AddPage(Page Page)
        {
            try
            {
                db.Page.Add(Page);
                db.SaveChanges();
                Permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
                return Page;
            }
            catch
            {
                throw;
            }
        }

        public Page UpdatePage(Page Page)
        {
            try
            {
                db.Entry(Page).State = EntityState.Modified;
                db.SaveChanges();
                Permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
                return Page;
            }
            catch
            {
                throw;
            }
        }

        public Page GetPage(int PageId)
        {
            try
            {
                Page page = db.Page.Find(PageId);
                if (page != null)
                {
                    List<Permission> permissions = Permissions.GetPermissions("Page", page.PageId).ToList();
                    page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
                }
                return page;
            }
            catch
            {
                throw;
            }
        }

        public void DeletePage(int PageId)
        {
            try
            {
                Page Page = db.Page.Find(PageId);
                Permissions.UpdatePermissions(Page.SiteId, "Page", PageId, "");
                db.Page.Remove(Page);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
