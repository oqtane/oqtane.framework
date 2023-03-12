using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
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
                page.PermissionList = permissions.Where(item => item.EntityId == page.PageId).ToList();
            }
            return pages;
        }

        public Page AddPage(Page page)
        {
            _db.Page.Add(page);
            _db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.PermissionList);
            return page;
        }

        public Page UpdatePage(Page page)
        {
            _db.Entry(page).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.PermissionList);
            return page;
        }

        public Page GetPage(int pageId)
        {
            return GetPage(pageId, true);
        }

        public Page GetPage(int pageId, bool tracking)
        {
            Page page;
            if (tracking)
            {
                page = _db.Page.Find(pageId);

            }
            else
            {
                page = _db.Page.AsNoTracking().FirstOrDefault(item => item.PageId == pageId);
            }
            if (page != null)
            {
                page.PermissionList = _permissions.GetPermissions(page.SiteId, EntityNames.Page, page.PageId)?.ToList();
            }
            return page;
        }

        public Page GetPage(int pageId, int userId)
        {
            Page page = _db.Page.Find(pageId);
            if (page != null)
            {
                Page personalized = _db.Page.FirstOrDefault(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == userId);
                if (personalized != null)
                {
                    page = personalized;
                }
                page.PermissionList = _permissions.GetPermissions(page.SiteId, EntityNames.Page, page.PageId)?.ToList();
            }
            return page;
        }

        public Page GetPage(string path, int siteId)
        {
            Page page = _db.Page.FirstOrDefault(item => item.Path == path && item.SiteId == siteId);
            if (page != null)
            {
                page.PermissionList = _permissions.GetPermissions(page.SiteId, EntityNames.Page, page.PageId)?.ToList();
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
