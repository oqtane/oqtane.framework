using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Server.Modules.HtmlText.Repository;

namespace Oqtane.Server.Modules.HtmlText.Controllers
{
    [Route("{site}/api/[controller]")]
    public class HtmlTextController : Controller
    {
        private IHtmlTextRepository htmltext;

        public HtmlTextController(IHtmlTextRepository HtmlText)
        {
            htmltext = HtmlText;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<HtmlTextInfo> Get()
        {
            return htmltext.GetHtmlText();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public HtmlTextInfo Get(int id)
        {
            return htmltext.GetHtmlText(id);
        }

        // POST api/<controller>
        [HttpPost]
        public HtmlTextInfo Post([FromBody] HtmlTextInfo HtmlText)
        {
            if (ModelState.IsValid)
            {
                HtmlText = htmltext.AddHtmlText(HtmlText);
            }
            return HtmlText;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public HtmlTextInfo Put(int id, [FromBody] HtmlTextInfo HtmlText)
        {
            if (ModelState.IsValid)
            {
                HtmlText = htmltext.UpdateHtmlText(HtmlText);
            }
            return HtmlText; 
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            htmltext.DeleteHtmlText(id);
        }
    }
}
