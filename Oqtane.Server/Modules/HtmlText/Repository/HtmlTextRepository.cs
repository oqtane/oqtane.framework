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

        public Models.HtmlText GetHtmlText(int moduleId)
        {
            return _db.HtmlText.FirstOrDefault(item => item.ModuleId == moduleId);
        }


        public Models.HtmlText AddHtmlText(Models.HtmlText htmlText)
        {
            _db.HtmlText.Add(htmlText);
            _db.SaveChanges();
            return htmlText;
        }

        public Models.HtmlText UpdateHtmlText(Models.HtmlText htmlText)
        {
            _db.Entry(htmlText).State = EntityState.Modified;
            _db.SaveChanges();
            return htmlText;
        }

        public void DeleteHtmlText(int moduleId)
        {
            Models.HtmlText htmlText = _db.HtmlText.FirstOrDefault(item => item.ModuleId == moduleId);
            if (htmlText != null) _db.HtmlText.Remove(htmlText);
            _db.SaveChanges();
        }
    }
}
