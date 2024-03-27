using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using System.Net;
using Oqtane.Documentation;
using System.Collections.Generic;
using Oqtane.Modules.HtmlText.Services;
using System.Threading.Tasks;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextController : ModuleControllerBase
    {
        private readonly IHtmlTextService _htmlTextService;

        public HtmlTextController(IHtmlTextService htmlTextService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _htmlTextService = htmlTextService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task<IEnumerable<Models.HtmlText>> Get(string moduleId)
        {
            if (int.TryParse(moduleId, out int ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _htmlTextService.GetHtmlTextsAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{moduleId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.HtmlText> Get(int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _htmlTextService.GetHtmlTextAsync(moduleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5/6
        [HttpGet("{id}/{moduleId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.HtmlText> Get(int id, int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _htmlTextService.GetHtmlTextAsync(id, moduleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Get Attempt {HtmlTextId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.HtmlText> Post([FromBody] Models.HtmlText htmlText)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, htmlText.ModuleId))
            {
                return await _htmlTextService.AddHtmlTextAsync(htmlText);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Post Attempt {HtmlText}", htmlText);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                await _htmlTextService.DeleteHtmlTextAsync(id, moduleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Delete Attempt {HtmlTextId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
