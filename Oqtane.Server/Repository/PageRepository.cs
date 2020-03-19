using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

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

        public IEnumerable<Page> GetPages(int siteId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.Page).ToList();
            IEnumerable<Page> pages = _db.Page.Where(item => item.SiteId == siteId && item.UserId == null);
            foreach(Page page in pages)
            {
                page.Permissions = _permissions.EncodePermissions(permissions.Where(item => item.EntityId == page.PageId));
            }
            return pages;
        }

        public Page AddPage(Page page)
        {
            _db.Page.Add(page);
            _db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.Permissions);
            return page;
        }

        public Page UpdatePage(Page page)
        {
            _db.Entry(page).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.Permissions);
            return page;
        }

        public Page GetPage(int pageId)
        {
            Page page = _db.Page.Find(pageId);
            if (page != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Page, page.PageId).ToList();
                page.Permissions = _permissions.EncodePermissions(permissions);
            }
            return page;
        }

        public Page GetPage(int pageId, int userId)
        {
            Page page = _db.Page.Find(pageId);
            if (page != null)
            {
                Page personalized = _db.Page.Where(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == userId).FirstOrDefault();
                if (personalized != null)
                {
                    page = personalized;
                }
                if (page != null)
                {
                    IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Page, page.PageId).ToList();
                    page.Permissions = _permissions.EncodePermissions(permissions);
                }
            }
            return page;
        }

        public Page GetPage(string path, int siteId)
        {
            Page page = _db.Page.Where(item => item.Path == path && item.SiteId == siteId).FirstOrDefault();
            if (page != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Page, page.PageId).ToList();
                page.Permissions = _permissions.EncodePermissions(permissions);
            }
            return page;
        }

        public void DeletePage(int pageId)
        {
            Page page = _db.Page.Find(pageId);
            _permissions.DeletePermissions(page.SiteId, EntityNames.Page, pageId);
            IEnumerable<PageModule> pageModules = _db.PageModule.Where(item => item.PageId == pageId).ToList();
            foreach (var pageModule in pageModules)
            {
                _pageModules.DeletePageModule(pageModule.PageModuleId);
            }
            _db.Page.Remove(page);
            _db.SaveChanges();
        }
    }
}
