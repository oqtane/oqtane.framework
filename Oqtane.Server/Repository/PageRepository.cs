using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageRepository : Repository<Page>, IPageRepository
    {
        private readonly TenantDBContext _context;
        private readonly IPermissionRepository _permissions;
        private readonly IPageModuleRepository _pageModules;

        public PageRepository(TenantDBContext context, IPermissionRepository permissions, IPageModuleRepository pageModules)
            :base(context)
        {
            _context = context;
            _permissions = permissions;
            _pageModules = pageModules;
        }

        public IEnumerable<Page> GetAll(int siteId)
        {
            var permissions = _permissions.GetPermissions(siteId, "Page").ToList();
            var pages = DbSet.Where(item => item.SiteId == siteId && item.UserId == null);
            foreach(Page page in pages)
            {
                page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
            }
            
            return pages;
        }

        public override Page Add(Page page)
        {
            page = base.Add(page);
            _permissions.UpdatePermissions(page.SiteId, "Page", page.PageId, page.Permissions);
            
            return page;
        }

        public override Page Update(Page page)
        {
            page = base.Update(page);
            _permissions.UpdatePermissions(page.SiteId, "Page", page.PageId, page.Permissions);
            
            return page;
        }

        public override Page Get(int id)
        {
            var page = base.Get(id);
            if (page != null)
            {
                var permissions = _permissions.GetPermissions("Page", page.PageId);
                page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
            }
            
            return page;
        }

        public Page Get(int id, int userId)
        {
            var page = base.Get(id);
            if (page != null)
            {
                var personalized = DbSet.Where(item => item.SiteId == page.SiteId && item.Path == page.Path && item.UserId == userId).FirstOrDefault();
                if (personalized != null)
                {
                    page = personalized;
                }
                
                if (page != null)
                {
                    var permissions = _permissions.GetPermissions("Page", page.PageId);
                    page.Permissions = _permissions.EncodePermissions(page.PageId, permissions);
                }
            }
            
            return page;
        }

        public override void Delete(int id)
        {
            var page = base.Get(id);
            _permissions.DeletePermissions(page.SiteId, "Page", id);
            var pageModules = _context.PageModule.Where(item => item.PageId == id).ToList();
            foreach (var pageModule in pageModules)
            {
                _pageModules.DeletePageModule(pageModule.PageModuleId);
            }

            base.Delete(id);
        }
    }
}
