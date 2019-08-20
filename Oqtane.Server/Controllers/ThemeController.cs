using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository Themes;

        public ThemeController(IThemeRepository Themes)
        {
            this.Themes = Themes;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Theme> Get()
        {
            return Themes.GetThemes();
        }
    }
}
