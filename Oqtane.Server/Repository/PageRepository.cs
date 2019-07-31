using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class PageRepository : IPageRepository
    {
        private TenantDBContext db;

        public PageRepository(TenantDBContext context)
        {
            db = context;
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
                return db.Page.Where(item => item.SiteId == SiteId).ToList();
            }
            catch
            {
                throw;
            }
        }

        public void AddPage(Page Page)
        {
            try
            {
                db.Page.Add(Page);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdatePage(Page Page)
        {
            try
            {
                db.Entry(Page).State = EntityState.Modified;
                db.SaveChanges();
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
                Page Page = db.Page.Find(PageId);
                return Page;
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
