using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Server.Modules.HtmlText.Repository;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Server.Modules.HtmlText.Controllers
{
    [Route("{site}/api/[controller]")]
    public class HtmlTextController : Controller
    {
        private IHtmlTextRepository htmltext;
        private int EntityId = -1; // passed as a querystring parameter for authorization and used for validation

        public HtmlTextController(IHtmlTextRepository HtmlText, IHttpContextAccessor HttpContextAccessor)
        {
            htmltext = HtmlText;
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
            }
        }
    }
}
