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

        public HtmlTextInfo GetHtmlText(int moduleId)
        {
            return _db.HtmlText.FirstOrDefault(item => item.ModuleId == moduleId);
        }


        public HtmlTextInfo AddHtmlText(HtmlTextInfo htmlText)
        {
            _db.HtmlText.Add(htmlText);
            _db.SaveChanges();
            return htmlText;
        }

        public HtmlTextInfo UpdateHtmlText(HtmlTextInfo htmlText)
        {
            _db.Entry(htmlText).State = EntityState.Modified;
            _db.SaveChanges();
            return htmlText;
        }

        public void DeleteHtmlText(int moduleId)
        {
            HtmlTextInfo htmlText = _db.HtmlText.FirstOrDefault(item => item.ModuleId == moduleId);
            if (htmlText != null) _db.HtmlText.Remove(htmlText);
            _db.SaveChanges();
        }
    }
}
