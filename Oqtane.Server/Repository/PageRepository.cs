using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageRepository : IPageRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;
        private readonly IPageModuleRepository _pageModules;

        public PageRepository(TenantDBContext context, IPermissionRepository permissions, IPageModuleRepository pageModules)
        {
            _db = context;
            _permissions = permissions;
            _pageModules = pageModules;
        }

        public IEnumerable<Page> GetPages(int SiteId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(SiteId, "Page").ToList();
            IEnumerable<Page> pages = _db.Page.Where(item => item.SiteId == SiteId && item.UserId == null);
            foreach(Page page in pages)
            {
                page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
            }
            return pages;
        }

        public Page AddPage(Page Page)
        {
            _db.Page.Add(Page);
            _db.SaveChanges();
            _permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
            return Page;
        }

        public Page UpdatePage(Page Page)
        {
            _db.Entry(Page).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(Page.SiteId, "Page", Page.PageId, Page.Permissions);
            return Page;
        }

        public Page GetPage(int PageId)
        {
            Page page = _db.Page.Find(PageId);
            if (page != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Page", page.PageId).ToList();
                page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
            }
            return page;
        }

        public Page GetPage(int PageId, int UserId)
        {
            Page page = _db.Page.Find(PageId);
            if (page != null)
            {
                Page personalized = _db.Page.Where(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == UserId).FirstOrDefault();
                if (personalized != null)
                {
                    page = personalized;
                }
                if (page != null)
                {
                    IEnumerable<Permission> permissions = _permissions.GetPermissions("Page", page.PageId).ToList();
                    page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
                }
            }
            return page;
        }

        public void DeletePage(int PageId)
        {
            Page Page = _db.Page.Find(PageId);
            _permissions.DeletePermissions(Page.SiteId, "Page", PageId);
            IEnumerable<PageModule> pageModules = _db.PageModule.Where(item => item.PageId == PageId).ToList();
            foreach (var pageModule in pageModules)
            {
                _pageModules.DeletePageModule(pageModule.PageModuleId);
            }
            _db.Page.Remove(Page);
            _db.SaveChanges();
        }
    }
}
