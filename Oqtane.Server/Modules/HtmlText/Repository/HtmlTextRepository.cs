using System.Linq;
using Oqtane.Documentation;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using System;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly HtmlTextContext _db;
        private readonly IMemoryCache _cache;
        private readonly SiteState _siteState;

        public HtmlTextRepository(HtmlTextContext context, IMemoryCache cache, SiteState siteState)
        {
            _db = context;
            _cache = cache;
            _siteState = siteState;
        }

        public IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId)
        {
            return _cache.GetOrCreate($"HtmlText:{_siteState.Alias.SiteKey}:{moduleId}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return _db.HtmlText.Where(item => item.ModuleId == moduleId).ToList();
            });
        }

        public Models.HtmlText GetHtmlText(int htmlTextId)
        {
            return _db.HtmlText.Find(htmlTextId);
        }

        public Models.HtmlText AddHtmlText(Models.HtmlText htmlText)
        {
            _db.HtmlText.Add(htmlText);
            _db.SaveChanges();
            ClearCache(htmlText.ModuleId);
            return htmlText;
        }

        public void DeleteHtmlText(int htmlTextId)
        {
            Models.HtmlText htmlText = _db.HtmlText.FirstOrDefault(item => item.HtmlTextId == htmlTextId);
            if (htmlText != null) _db.HtmlText.Remove(htmlText);
            ClearCache(htmlText.ModuleId);
            _db.SaveChanges();
        }

        private void ClearCache(int moduleId)
        {
            _cache.Remove($"HtmlText:{_siteState.Alias.SiteKey}:{moduleId}");
        }
    }
}
