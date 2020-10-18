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
using Oqtane.Controllers;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route(ControllerRoutes.Default)]
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
        [Authorize(Policy = PolicyNames.EditModule)]
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
        [Authorize(Policy = PolicyNames.EditModule)]
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
        [Authorize(Policy = PolicyNames.EditModule)]
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
