using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageModuleRepository : IPageModuleRepository
    {
        private TenantDBContext db;

        public PageModuleRepository(TenantDBContext context)
        {
            db = context;
        }

        public IEnumerable<PageModule> GetPageModules()
        {
            try
            {
                return db.PageModule.ToList();
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<PageModule> GetPageModules(int PageId)
        {
            try
            {
                List<PageModule> pagemodules = db.PageModule.Where(item => item.PageId == PageId)
                    .Include(item => item.Module)
                    .ToList();
                return pagemodules;
            }
            catch
            {
                throw;
            }
        }

        public PageModule AddPageModule(PageModule PageModule)
        {
            try
            {
                db.PageModule.Add(PageModule);
                db.SaveChanges();
                return PageModule;
            }
            catch
            {
                throw;
            }
        }

        public PageModule UpdatePageModule(PageModule PageModule)
        {
            try
            {
                db.Entry(PageModule).State = EntityState.Modified;
                db.SaveChanges();
                return PageModule;
            }
            catch
            {
                throw;
            }
        }

        public PageModule GetPageModule(int PageModuleId)
        {
            try
            {
                PageModule PageModule = db.PageModule.Find(PageModuleId);
                return PageModule;
            }
            catch
            {
                throw;
            }
        }

        public void DeletePageModule(int PageModuleId)
        {
            try
            {
                PageModule PageModule = db.PageModule.Find(PageModuleId);
                db.PageModule.Remove(PageModule);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
