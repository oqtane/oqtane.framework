using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Documentation;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId);
        Models.HtmlText GetHtmlText(int moduleId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int htmlTextId);
    }

    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly IDbContextFactory<HtmlTextContext> _factory;
        private readonly ISettingRepository _settingRepository;

        public HtmlTextRepository(IDbContextFactory<HtmlTextContext> factory, ISettingRepository settingRepository)
        {
            _factory = factory;
            _settingRepository = settingRepository;
        }

        public IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Where(item => item.ModuleId == moduleId).ToList();
        }

        public Models.HtmlText GetHtmlText(int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Where(item => item.ModuleId == moduleId)?
                .OrderByDescending(item => item.CreatedOn).FirstOrDefault();
        }

        public Models.HtmlText AddHtmlText(Models.HtmlText htmlText)
        {
            using var db = _factory.CreateDbContext();

            var versions = int.Parse(_settingRepository.GetSettingValue(EntityNames.Module, htmlText.ModuleId, "Versions", "5"));
            if (versions > 0)
            {
                var htmlTexts = db.HtmlText.Where(item => item.ModuleId == htmlText.ModuleId).OrderByDescending(item => item.CreatedOn).ToList();
                for (int i = versions - 1; i < htmlTexts.Count; i++)
                {
                    db.HtmlText.Remove(htmlTexts[i]);
                }
            }

            db.HtmlText.Add(htmlText);
            db.SaveChanges();
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
            }
        }
    }
}
