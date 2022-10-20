using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class LocalizationController : Controller
    {
        private readonly ILocalizationManager _localizationManager;

        public LocalizationController(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        // GET: api/localization
        [HttpGet()]
        public IEnumerable<Culture> Get(bool installed)
        {
            string[] culturecodes;
            if (installed)
            {
                culturecodes = _localizationManager.GetInstalledCultures();
            }
            else
            {
                culturecodes = _localizationManager.GetSupportedCultures();
            }
            var cultures = culturecodes.Select(c => new Culture
               {
                   Name = CultureInfo.GetCultureInfo(c).Name,
                   DisplayName = CultureInfo.GetCultureInfo(c).DisplayName,
                   IsDefault = _localizationManager.GetDefaultCulture()
                    .Equals(CultureInfo.GetCultureInfo(c).Name, StringComparison.OrdinalIgnoreCase)
               });
            return cultures.OrderBy(item => item.DisplayName);
        }
    }
}
