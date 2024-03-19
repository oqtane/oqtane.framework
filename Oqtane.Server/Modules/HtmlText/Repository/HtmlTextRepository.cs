using System.Linq;
using Oqtane.Documentation;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextRepository : IHtmlTextRepository, ITransientService
    {
        private readonly HtmlTextContext _db;
        private readonly IDbContextFactory<HtmlTextContext> _factory;
        private readonly IMemoryCache _cache;
        private readonly SiteState _siteState;

        public HtmlTextRepository(HtmlTextContext context, IDbContextFactory<HtmlTextContext> factory, IMemoryCache cache, SiteState siteState)
        {
            _db = context;
            _factory = factory;
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

        public async Task<IEnumerable<Models.HtmlText>> GetHtmlTextsAsync(int moduleId)
        {
            return await _cache.GetOrCreateAsync($"HtmlText:{_siteState.Alias.SiteKey}:{moduleId}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                using var ctx = _factory.CreateDbContext();
                return await ctx.HtmlText.Where(item => item.ModuleId == moduleId).ToListAsync();
            });
        }

        public async Task<Models.HtmlText> GetHtmlTextAsync(int htmlTextId)
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.HtmlText.FindAsync(htmlTextId);
        }

        public async Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmlText)
        {
            using var ctx = _factory.CreateDbContext();
            ctx.HtmlText.Add(htmlText);
            await ctx.SaveChangesAsync();
            ClearCache(htmlText.ModuleId);
            return htmlText;
        }

        public async Task DeleteHtmlTextAsync(int htmlTextId)
        {
            using var ctx = _factory.CreateDbContext();
            Models.HtmlText htmlText = ctx.HtmlText.FirstOrDefault(item => item.HtmlTextId == htmlTextId);
            ctx.HtmlText.Remove(htmlText);
            ClearCache(htmlText.ModuleId);
            await ctx.SaveChangesAsync();
        }

        private void ClearCache(int moduleId)
        {
            _cache.Remove($"HtmlText:{_siteState.Alias.SiteKey}:{moduleId}");
        }
    }
}
