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

        public HtmlTextInfo GetHtmlText(int ModuleId)
        {
            try
            {
                return db.HtmlText.Where(item => item.ModuleId == ModuleId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }


        public HtmlTextInfo AddHtmlText(HtmlTextInfo HtmlText)
        {
            try
            {
                db.HtmlText.Add(HtmlText);
                db.SaveChanges();
                return HtmlText;
            }
            catch
            {
                throw;
            }
        }

        public HtmlTextInfo UpdateHtmlText(HtmlTextInfo HtmlText)
        {
            try
            {
                db.Entry(HtmlText).State = EntityState.Modified;
                db.SaveChanges();
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
