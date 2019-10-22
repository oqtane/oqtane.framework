using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using Oqtane.Shared;

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
            HtmlTextInfo HtmlText = null;
            if (EntityId == id)
            {
               HtmlText = htmltext.GetHtmlText(id);
            }
            return HtmlText;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Post([FromBody] HtmlTextInfo HtmlText)
        {
            if (ModelState.IsValid && HtmlText.ModuleId == EntityId)
            {
                HtmlText = htmltext.AddHtmlText(HtmlText);
                logger.AddLog(this.GetType().FullName, LogLevel.Information, "Html/Text Added {HtmlText}", HtmlText);
            }
            return HtmlText;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = "EditModule")]
        public HtmlTextInfo Put(int id, [FromBody] HtmlTextInfo HtmlText)
        {
            if (ModelState.IsValid && HtmlText.ModuleId == EntityId)
            {
                HtmlText = htmltext.UpdateHtmlText(HtmlText);
                logger.AddLog(this.GetType().FullName, LogLevel.Information, "Html/Text Updated {HtmlText}", HtmlText);
            }
            return HtmlText; 
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "EditModule")]
        public void Delete(int id)
        {
            if (id == EntityId)
            {
                htmltext.DeleteHtmlText(id);
                logger.AddLog(this.GetType().FullName, LogLevel.Information, "Html/Text Deleted {HtmlTextId}", id);
            }
        }
    }
}
