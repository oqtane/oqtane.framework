using System.Linq;
using Oqtane.Documentation;
using System.Collections.Generic;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly HtmlTextContext _db;

        public HtmlTextRepository(HtmlTextContext context)
        {
            _db = context;
        }

        public IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId)
        {
            return _db.HtmlText.Where(item => item.ModuleId == moduleId);
        }

        public Models.HtmlText GetHtmlText(int htmlTextId)
        {
            return _db.HtmlText.Find(htmlTextId);
        }

        public Models.HtmlText AddHtmlText(Models.HtmlText htmlText)
        {
            _db.HtmlText.Add(htmlText);
            _db.SaveChanges();
            return htmlText;
        }

        public void DeleteHtmlText(int htmlTextId)
        {
            Models.HtmlText htmlText = _db.HtmlText.FirstOrDefault(item => item.HtmlTextId == htmlTextId);
            if (htmlText != null) _db.HtmlText.Remove(htmlText);
            _db.SaveChanges();
        }
    }
}
