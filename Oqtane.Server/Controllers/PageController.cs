using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageController : Controller
    {
        private readonly IPageRepository pages;

        public PageController(IPageRepository Pages)
        {
            pages = Pages;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid)
        {
            if (siteid == "")
            {
                return pages.GetPages();
            }
            else
            {
                return pages.GetPages(int.Parse(siteid));
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Page Get(int id)
        {
            return pages.GetPage(id);
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody] Page Page)
        {
            if (ModelState.IsValid)
                pages.AddPage(Page);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Page Page)
        {
            if (ModelState.IsValid)
                pages.UpdatePage(Page);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            pages.DeletePage(id);
        }
    }
}
