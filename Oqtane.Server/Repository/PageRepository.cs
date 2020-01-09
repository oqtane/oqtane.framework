using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageRepository : IPageRepository
    {
        private readonly TenantDBContext db;
        private readonly IPermissionRepository Permissions;
        private readonly IPageModuleRepository PageModules;

        public PageRepository(TenantDBContext context, IPermissionRepository Permissions, IPageModuleRepository PageModules)
        {
            db = context;
            this.Permissions = Permissions;
            this.PageModules = PageModules;
        }

        public IEnumerable<Page> GetAll()
        {
            return db.Page.ToList();
        }

        public IEnumerable<Page> GetAll(int siteId)
        {
            var permissions = Permissions.GetPermissions(siteId, "Page").ToList();
            var pages = db.Page.Where(item => item.SiteId == siteId && item.UserId == null);
            foreach(Page page in pages)
            {
                page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
            }
            
            return pages;
        }

        public Page Add(Page page)
        {
            db.Page.Add(page);
            db.SaveChanges();
            Permissions.UpdatePermissions(page.SiteId, "Page", page.PageId, page.Permissions);
            
            return page;
        }

        public Page Update(Page page)
        {
            db.Entry(page).State = EntityState.Modified;
            db.SaveChanges();
            Permissions.UpdatePermissions(page.SiteId, "Page", page.PageId, page.Permissions);
            
            return page;
        }

        public Page Get(int id)
        {
            var page = db.Page.Find(id);
            if (page != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Page", page.PageId);
                page.Permissions = Permissions.EncodePermissions(page.PageId, permissions);
            }
            
            return page;
        }

        public Page Get(int id, int userId)
        {
            Page page = db.Page.Find(id);
            if (page != null)
            {
                Page personalized = db.Page.Where(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == userId).FirstOrDefault();
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

        public void Delete(int id)
        {
            var page = db.Page.Find(id);
            Permissions.DeletePermissions(page.SiteId, "Page", id);
            var pageModules = db.PageModule.Where(item => item.PageId == id).ToList();
            foreach (var pageModule in pageModules)
            {
                PageModules.DeletePageModule(pageModule.PageModuleId);
            }
            
            db.Page.Remove(page);
            db.SaveChanges();
        }
    }
}
