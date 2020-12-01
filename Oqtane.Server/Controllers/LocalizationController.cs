using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class LocalizationController : Controller
    {
        private readonly ILocalizationManager _localizationManager;

        public LocalizationController(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        // GET: api/localization/getSupportedCultures
        [HttpGet("getSupportedCultures")]
        public IEnumerable<string> GetSupportedCultures() => _localizationManager.GetSupportedCultures();

        // GET api/localization/getDefaultCulture
        [HttpGet("getDefaultCulture")]
        public string GetDefaultCulture() => _localizationManager.GetDefaultCulture();
    }
}
