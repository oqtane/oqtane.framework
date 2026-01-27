using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class LocalizationController : Controller
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ISiteRepository _siteRepository;
        private readonly ISiteGroupRepository _siteGroupRepository;
        private readonly IAliasRepository _aliasRepository;

        public LocalizationController(ILocalizationManager localizationManager, ISiteRepository siteRepository, ISiteGroupRepository siteGroupRepository, IAliasRepository aliasRepository)
        {
            _localizationManager = localizationManager;
            _siteRepository = siteRepository;
            _siteGroupRepository = siteGroupRepository;
            _aliasRepository = aliasRepository;
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
               }).ToList();

            if (cultures.Count == 0)
            {
                cultures.Add(new Culture { Name = "en", DisplayName = "English", IsDefault = true });
            }

            return cultures.OrderBy(item => item.DisplayName);
        }
    }
}
