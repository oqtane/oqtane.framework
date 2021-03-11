using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class LanguageController : Controller
    {
        private readonly ILanguageRepository _languages;
        private readonly ILogManager _logger;

        public LanguageController(ILanguageRepository language, ILogManager logger)
        {
            _languages = language;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Language> Get(string siteid) => _languages.GetLanguages(int.Parse(siteid));

        [HttpGet("{id}")]
        public Language Get(int id) => _languages.GetLanguage(id);

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public Language Post([FromBody] Language language)
        {
            if (ModelState.IsValid)
            {
                language = _languages.AddLanguage(language);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Language Added {Language}", language);
            }
            return language;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            _languages.DeleteLanguage(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Language Deleted {LanguageId}", id);
        }
    }
}
