using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    public class ModuleControllerBase : Controller
    {
        protected readonly ILogManager _logger;
        protected int _entityId = -1; // passed as a querystring parameter for policy authorization and used for validation

        public ModuleControllerBase(ILogManager logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            if (accessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                _entityId = int.Parse(accessor.HttpContext.Request.Query["entityid"]);
            }
        }
    }
}
