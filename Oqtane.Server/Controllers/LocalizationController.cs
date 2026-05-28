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

        // GET: api/localization?installed=true/false
        [HttpGet()]
        public IEnumerable<Culture> Get(bool installed)
        {
            string[] cultureCodes;
            if (installed)
            {
                cultureCodes = _localizationManager.GetInstalledCultures();
            }
            else
            {
                cultureCodes = _localizationManager.GetSupportedCultures();
            }

            var cultures = cultureCodes.Select(c => new Culture
               {
                   Name = CultureInfo.GetCultureInfo(c).Name,
                   DisplayName = CultureInfo.GetCultureInfo(c).DisplayName,
                   IsDefault = _localizationManager.GetDefaultCulture()
                    .Equals(CultureInfo.GetCultureInfo(c).Name, StringComparison.OrdinalIgnoreCase)
               }).ToList();

            if (cultures.Count == 0)
            {
                cultures.Add(new Culture { Name = "en", DisplayName = "English", IsDefault = true });
            }

            return cultures.OrderBy(item => item.DisplayName);
        }

        // GET: api/localization/neutral
        [HttpGet("neutral")]
        public IEnumerable<Culture> Get()
        {
            var cultureCodes = _localizationManager.GetNeutralCultures();

            var cultures = cultureCodes.Select(c => new Culture
            {
                Name = CultureInfo.GetCultureInfo(c).Name,
                DisplayName = CultureInfo.GetCultureInfo(c).DisplayName,
                IsDefault = false
            }).ToList();

            if (cultures.Count == 0)
            {
                cultures.Add(new Culture { Name = "en", DisplayName = "English", IsDefault = false });
            }

            return cultures.OrderBy(item => item.DisplayName);
        }
    }
}
