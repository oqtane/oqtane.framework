using System.Linq;
using Oqtane.Documentation;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Oqtane.Repository;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId);
        Models.HtmlText GetHtmlText(int htmlTextId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int htmlTextId);
    }

    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly IDbContextFactory<HtmlTextContext> _factory;
        private readonly IModuleRepository _moduleRepository;

        public HtmlTextRepository(IDbContextFactory<HtmlTextContext> factory, IModuleRepository moduleRepository)
        {
            _factory = factory;
            _moduleRepository = moduleRepository;
        }

        public IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Where(item => item.ModuleId == moduleId).ToList();
        }

        public Models.HtmlText GetHtmlText(int htmlTextId)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Find(htmlTextId);
        }

        public Models.HtmlText AddHtmlText(Models.HtmlText htmlText)
        {
            using var db = _factory.CreateDbContext();
            db.HtmlText.Add(htmlText);
            db.SaveChanges();

            // update module ModifiedOn date
            var module = _moduleRepository.GetModule(htmlText.ModuleId);
            _moduleRepository.UpdateModule(module);

            return htmlText;
        }

        public void DeleteHtmlText(int htmlTextId)
        {
            using var db = _factory.CreateDbContext();
            Models.HtmlText htmlText = db.HtmlText.FirstOrDefault(item => item.HtmlTextId == htmlTextId);
            if (htmlText != null)
            {
                db.HtmlText.Remove(htmlText);
                db.SaveChanges();

                // update module ModifiedOn date
                var module = _moduleRepository.GetModule(htmlText.ModuleId);
                _moduleRepository.UpdateModule(module);
            }
        }
    }
}
