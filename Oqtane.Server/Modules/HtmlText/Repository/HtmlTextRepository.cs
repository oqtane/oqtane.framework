using Microsoft.EntityFrameworkCore;
using System.Linq;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextRepository : IHtmlTextRepository, IService
    {
        private readonly HtmlTextContext _db;

        public HtmlTextRepository(HtmlTextContext context)
        {
            _db = context;
        }

        public HtmlTextInfo GetHtmlText(int ModuleId)
        {
            try
            {
                return _db.HtmlText.Where(item => item.ModuleId == ModuleId).FirstOrDefault();
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
                _db.HtmlText.Add(HtmlText);
                _db.SaveChanges();
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
                _db.Entry(HtmlText).State = EntityState.Modified;
                _db.SaveChanges();
                return HtmlText;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteHtmlText(int ModuleId)
        {
            try
            {
                HtmlTextInfo HtmlText = _db.HtmlText.Where(item => item.ModuleId == ModuleId).FirstOrDefault();
                _db.HtmlText.Remove(HtmlText);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}
