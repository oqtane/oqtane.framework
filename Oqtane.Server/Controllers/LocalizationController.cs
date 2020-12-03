using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using Oqtane.Models;
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
        public IEnumerable<Culture> GetSupportedCultures()
            => _localizationManager.GetSupportedCultures().Select(c => new Culture {
                Name = CultureInfo.GetCultureInfo(c).Name,
                DisplayName = CultureInfo.GetCultureInfo(c).DisplayName
            });

        // GET api/localization/getDefaultCulture
        [HttpGet("getDefaultCulture")]
        public Culture GetDefaultCulture()
        {
            var culture = _localizationManager.GetDefaultCulture();

            return new Culture
            {
                Name = CultureInfo.GetCultureInfo(culture).Name,
                DisplayName = CultureInfo.GetCultureInfo(culture).DisplayName
            };
        }
    }
}
