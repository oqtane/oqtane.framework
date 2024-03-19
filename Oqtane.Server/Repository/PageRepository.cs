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
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IPageModuleRepository _pageModules;
        private readonly IPermissionRepository _permissions;
        private readonly ISettingRepository _settings;

        public PageRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IPageModuleRepository pageModules, IPermissionRepository permissions, ISettingRepository settings)
        {
            _dbContextFactory = dbContextFactory;
            _pageModules = pageModules;
            _permissions = permissions;
            _settings = settings;
        }

        public IEnumerable<Page> GetPages(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var permissions = _permissions.GetPermissions(siteId, EntityNames.Page).ToList();
            var pages = db.Page.Where(item => item.SiteId == siteId && item.UserId == null).ToList();
            foreach (var page in pages)
            {
                page.PermissionList = permissions.Where(item => item.EntityId == page.PageId).ToList();
            }
            return pages;
        }

        public Page AddPage(Page page)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Page.Add(page);
            db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.PermissionList);
            return page;
        }

        public Page UpdatePage(Page page)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(page).State = EntityState.Modified;
            db.SaveChanges();
            _permissions.UpdatePermissions(page.SiteId, EntityNames.Page, page.PageId, page.PermissionList);
            return page;
        }

        public Page GetPage(int pageId)
        {
            return GetPage(pageId, true);
        }

        public Page GetPage(int pageId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            Page page;
            if (tracking)
            {
                page = db.Page.Find(pageId);

            }
            else
            {
                page = db.Page.AsNoTracking().FirstOrDefault(item => item.PageId == pageId);
            }
            if (page != null)
            {
                page.PermissionList = _permissions.GetPermissions(page.SiteId, EntityNames.Page, page.PageId)?.ToList();
            }
            return page;
        }

        public Page GetPage(string path, int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var page = db.Page.FirstOrDefault(item => item.Path == path && item.SiteId == siteId);
            if (page != null)
            {
                page.PermissionList = _permissions.GetPermissions(page.SiteId, EntityNames.Page, page.PageId)?.ToList();
            }
            return page;
        }

        public void DeletePage(int pageId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var page = db.Page.Find(pageId);
            _permissions.DeletePermissions(page.SiteId, EntityNames.Page, pageId);
            _settings.DeleteSettings(EntityNames.Page, pageId);
            // remove page modules for page
            var pageModules = db.PageModule.Where(item => item.PageId == pageId).ToList();
            foreach (var pageModule in pageModules)
            {
                _pageModules.DeletePageModule(pageModule.PageModuleId);
            }
            // must occur after page modules are deleted because of cascading delete relationship
            db.Page.Remove(page);
            db.SaveChanges();
        }
    }
}
