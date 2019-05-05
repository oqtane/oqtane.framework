using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Modules;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public class HtmlTextRepository : IHtmlTextRepository, IService
    {
        private readonly HtmlTextContext db;

        public HtmlTextRepository(HtmlTextContext context)
        {
            db = context;
        }

        public IEnumerable<HtmlTextInfo> GetHtmlText()
        {
            try
            {
                return db.HtmlText.ToList();
            }
            catch
            {
                throw;
            }
        }

        public void AddHtmlText(HtmlTextInfo HtmlText)
        {
            try
            {
                db.HtmlText.Add(HtmlText);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateHtmlText(HtmlTextInfo HtmlText)
        {
            try
            {
                db.Entry(HtmlText).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public HtmlTextInfo GetHtmlText(int HtmlTextId)
        {
            try
            {
                HtmlTextInfo HtmlText = db.HtmlText.Find(HtmlTextId);
                return HtmlText;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteHtmlText(int HtmlTextId)
        {
            try
            {
                HtmlTextInfo HtmlText = db.HtmlText.Find(HtmlTextId);
                db.HtmlText.Remove(HtmlText);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
