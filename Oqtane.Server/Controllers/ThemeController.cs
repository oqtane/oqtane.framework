using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository Themes;
        private readonly IInstallation Installation;

        public ThemeController(IThemeRepository Themes, IInstallation Installation)
        {
            this.Themes = Themes;
            this.Installation = Installation;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Theme> Get()
        {
            return Themes.GetThemes();
        }

        [HttpGet("install")]
        [Authorize(Roles = Constants.HostRole)]
        public void InstallThemes()
        {
            Installation.Install("Themes");
        }
    }
}
