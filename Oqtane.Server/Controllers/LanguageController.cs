using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class LanguageController : Controller
    {
        private readonly ILanguageRepository _languages;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public LanguageController(ILanguageRepository language, ILogManager logger, ITenantManager tenantManager)
        {
            _languages = language;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        [HttpGet]
        public IEnumerable<Language> Get(string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                return _languages.GetLanguages(SiteId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpGet("{id}")]
        public Language Get(int id)
        {
            var language = _languages.GetLanguage(id);
            if (language != null && language.SiteId == _alias.SiteId)
            {
                return language;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Get Attempt {LanguageId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public Language Post([FromBody] Language language)
        {
            if (ModelState.IsValid && language.SiteId == _alias.SiteId)
            {
                language = _languages.AddLanguage(language);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Language Added {Language}", language);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Post Attempt {Language}", language);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                language = null;
            }
            return language;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            var language = _languages.GetLanguage(id);
            if (language != null && language.SiteId == _alias.SiteId)
            {
                _languages.DeleteLanguage(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Language Deleted {LanguageId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Delete Attempt {LanguageId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
    }
}
