using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface IPageRepository
    {
        IEnumerable<Page> GetPages(int siteId);
        Page AddPage(Page page);
        Page UpdatePage(Page page);
        Page GetPage(int pageId);
        Page GetPage(int pageId, bool tracking);
        Page GetPage(string path, int siteId);
        void DeletePage(int pageId);
    }

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
            return GetPagesHierarchy(pages);
        }

        private static List<Page> GetPagesHierarchy(List<Page> pages)
        {
            List<Page> hierarchy = new List<Page>();
            Action<List<Page>, Page> getPath = null;
            getPath = (pageList, page) =>
            {
                IEnumerable<Page> children;
                int level;
                if (page == null)
                {
                    level = -1;
                    children = pages.Where(item => item.ParentId == null);
                }
                else
                {
                    level = page.Level;
                    children = pages.Where(item => item.ParentId == page.PageId);
                }
                foreach (Page child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = pages.Any(item => item.ParentId == child.PageId && !item.IsDeleted && item.IsNavigation);
                    hierarchy.Add(child);
                    getPath(pageList, child);
                }
            };
            pages = pages.OrderBy(item => item.Order).ToList();
            getPath(pages, null);

            // add any non-hierarchical items to the end of the list
            foreach (Page page in pages)
            {
                if (hierarchy.Find(item => item.PageId == page.PageId) == null)
                {
                    hierarchy.Add(page);
                }
            }
            return hierarchy;
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
            {
                var page = db.Page.Find(pageId);
                _permissions.DeletePermissions(page.SiteId, EntityNames.Page, pageId);
                _settings.DeleteSettings(EntityNames.Page, pageId);
                // remove page modules for page
                var pageModules = db.PageModule.Where(item => item.PageId == pageId).ToList();
                foreach (var pageModule in pageModules)
                {
                    _pageModules.DeletePageModule(pageModule.PageModuleId);
                }

                // At this point the page item is unaware of changes happened in other
                // contexts (i.e.: the contex opened and closed in each DeletePageModule).
                // Workin on page item may result in unxpected behaviour:
                // better close and reopen context to work on a fresh page item.
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            {
                var page = dbContext.Page.Find(pageId);
                dbContext.Page.Remove(page);
                dbContext.SaveChanges();
            }
        }
    }
}
