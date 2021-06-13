using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using System.Net;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class HtmlTextController : ModuleControllerBase
    {
        private readonly IHtmlTextRepository _htmlText;

        public HtmlTextController(IHtmlTextRepository htmlText, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _htmlText = htmlText;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Models.HtmlText Get(int id)
        {
            if (AuthEntityId(EntityNames.Module) == id)
            {
                return _htmlText.GetHtmlText(id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HtmlText Get Attempt {ModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.HtmlText Post([FromBody] Models.HtmlText htmlText)
        {
            if (ModelState.IsValid && AuthEntityId(EntityNames.Module) == htmlText.ModuleId)
            {
                htmlText = _htmlText.AddHtmlText(htmlText);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Html/Text Added {HtmlText}", htmlText);
                return htmlText;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HtmlText Post Attempt {HtmlText}", htmlText);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/5
        [ValidateAntiForgeryToken]
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.HtmlText Put(int id, [FromBody] Models.HtmlText htmlText)
        {
            if (ModelState.IsValid && AuthEntityId(EntityNames.Module) == htmlText.ModuleId)
            {
                htmlText = _htmlText.UpdateHtmlText(htmlText);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Html/Text Updated {HtmlText}", htmlText);
                return htmlText;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HtmlText Put Attempt {HtmlText}", htmlText);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // DELETE api/<controller>/5
        [ValidateAntiForgeryToken]
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public void Delete(int id)
        {
            if (AuthEntityId(EntityNames.Module) == id)
            {
                _htmlText.DeleteHtmlText(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Html/Text Deleted {HtmlTextId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HtmlText Delete Attempt {ModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
