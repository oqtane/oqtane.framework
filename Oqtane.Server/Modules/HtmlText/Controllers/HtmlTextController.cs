using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System;

namespace Oqtane.Modules.HtmlText.Controllers
{
    [Route("{site}/api/[controller]")]
    public class HtmlTextController : Controller
    {
        private readonly IHtmlTextRepository htmltext;
        private readonly ILogManager logger;
        private int EntityId = -1; // passed as a querystring parameter for authorization and used for validation

        public HtmlTextController(IHtmlTextRepository HtmlText, ILogManager logger, IHttpContextAccessor HttpContextAccessor)
        {
            htmltext = HtmlText;
            this.logger = logger;
            if (HttpContextAccessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                EntityId = int.Parse(HttpContextAccessor.HttpContext.Request.Query["entityid"]);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewModule")]
        public HtmlTextInfo Get(int id)
        {
            try
            {
                HtmlTextInfo HtmlText = null;
                if (EntityId == id)
                {
                    HtmlText = htmltext.GetHtmlText(id);
                }
                return HtmlText;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Error}", ex.Message);
                throw;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Post([FromBody] HtmlTextInfo HtmlText)
        {
            try
            {
                if (ModelState.IsValid && HtmlText.ModuleId == EntityId)
                {
                    HtmlText = htmltext.AddHtmlText(HtmlText);
                    logger.Log(LogLevel.Information, this, LogFunction.Create, "Html/Text Added {HtmlText}", HtmlText);
                }
                return HtmlText;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Post Error {Error}", ex.Message);
                throw;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Put(int id, [FromBody] HtmlTextInfo HtmlText)
        {
            try
            {
                if (ModelState.IsValid && HtmlText.ModuleId == EntityId)
                {
                    HtmlText = htmltext.UpdateHtmlText(HtmlText);
                    logger.Log(LogLevel.Information, this, LogFunction.Update, "Html/Text Updated {HtmlText}", HtmlText);
                }
                return HtmlText;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Put Error {Error}", ex.Message);
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
                if (id == EntityId)
                {
                    htmltext.DeleteHtmlText(id);
                    logger.Log(LogLevel.Information, this, LogFunction.Delete, "Html/Text Deleted {HtmlTextId}", id);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {Error}", ex.Message);
                throw;
            }
        }
    }
}
