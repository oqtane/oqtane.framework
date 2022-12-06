using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using System.Net;
using Oqtane.Documentation;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextController : ModuleControllerBase
    {
        private readonly IHtmlTextRepository _htmlText;

        public HtmlTextController(IHtmlTextRepository htmlText, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _htmlText = htmlText;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Models.HtmlText> Get(string moduleId)
        {
            if (int.TryParse(moduleId, out int ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return _htmlText.GetHtmlTexts(ModuleId);
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
        public Models.HtmlText Get(int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                var htmltexts = _htmlText.GetHtmlTexts(moduleId);
                if (htmltexts != null && htmltexts.Any())
                {
                    return htmltexts.OrderByDescending(item => item.CreatedOn).First();
                }
                else
                {
                    return null;
                }
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
        public Models.HtmlText Get(int id, int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return _htmlText.GetHtmlText(id);
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
        public Models.HtmlText Post([FromBody] Models.HtmlText htmlText)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, htmlText.ModuleId))
            {
                htmlText = _htmlText.AddHtmlText(htmlText);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Html/Text Added {HtmlText}", htmlText);
                return htmlText;
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
        public void Delete(int id, int moduleId)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                _htmlText.DeleteHtmlText(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Html/Text Deleted {HtmlTextId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Html/Text Delete Attempt {HtmlTextId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
