using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SkinController : Controller
    {
        private readonly ISkinRepository skins;

        public SkinController(ISkinRepository Skins)
        {
            skins = Skins;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Skin> Get()
        {
            return skins.GetSkins();
        }
    }
}
