using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Documentation;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId);
        Models.HtmlText GetHtmlText(int moduleId);
        Models.HtmlText GetHtmlText(int moduleId, int status);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        Models.HtmlText UpdateHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int htmlTextId);
    }

    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly IDbContextFactory<HtmlTextContext> _factory;
        private readonly ISettingRepository _settingRepository;
        private readonly IHttpContextAccessor _accessor;

        public HtmlTextRepository(IDbContextFactory<HtmlTextContext> factory, ISettingRepository settingRepository, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _settingRepository = settingRepository;
            _accessor = accessor;
        }

        public IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Where(item => item.ModuleId == moduleId).ToList();
        }

        public Models.HtmlText GetHtmlText(int moduleId)
        {
            return GetHtmlText(moduleId, WorkflowState.Published);
        }

        public Models.HtmlText GetHtmlText(int moduleId, int state)
        {
            using var db = _factory.CreateDbContext();
            return db.HtmlText.Where(item => item.ModuleId == moduleId && item.State <= state)?
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

        public Models.HtmlText UpdateHtmlText(Models.HtmlText htmlText)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(htmlText).State = EntityState.Modified;
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
