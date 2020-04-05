using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using Oqtane.Enums;
using Oqtane.Infrastructure;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route("{site}/api/[controller]")]
    public class HtmlTextController : Controller
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly ILogManager _logger;
        private int _entityId = -1; // passed as a querystring parameter for authorization and used for validation

        public HtmlTextController(IHtmlTextRepository htmlText, ILogManager logger, IHttpContextAccessor httpContextAccessor)
        {
            _htmlText = htmlText;
            _logger = logger;
            if (httpContextAccessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                _entityId = int.Parse(httpContextAccessor.HttpContext.Request.Query["entityid"]);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewModule")]
        public List<HtmlTextInfo> Get(int id)
        {
            var list = new List<HtmlTextInfo>();
            try
            {
                HtmlTextInfo htmlText = null;
                if (_entityId == id)
                {
                    htmlText = _htmlText.GetHtmlText(id);
                    list.Add(htmlText);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Error}", ex.Message);
                throw;
            }
            return list;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Post([FromBody] HtmlTextInfo htmlText)
        {
            try
            {
                if (ModelState.IsValid && htmlText.ModuleId == _entityId)
                {
                    htmlText = _htmlText.AddHtmlText(htmlText);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Html/Text Added {HtmlText}", htmlText);
                }
                return htmlText;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Post Error {Error}", ex.Message);
                throw;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Put(int id, [FromBody] HtmlTextInfo htmlText)
        {
            try
            {
                if (ModelState.IsValid && htmlText.ModuleId == _entityId)
                {
                    htmlText = _htmlText.UpdateHtmlText(htmlText);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Html/Text Updated {HtmlText}", htmlText);
                }
                return htmlText;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Put Error {Error}", ex.Message);
                throw;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "EditModule")]
        public void Delete(int id)
        {
            try
            {
                if (id == _entityId)
                {
                    _htmlText.DeleteHtmlText(id);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Html/Text Deleted {HtmlTextId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {Error}", ex.Message);
                throw;
            }
        }
    }
}
