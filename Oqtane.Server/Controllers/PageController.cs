using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageController : Controller
    {
        private readonly IPageRepository Pages;

        public PageController(IPageRepository Pages)
        {
            this.Pages = Pages;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid)
        {
            if (siteid == "")
            {
                return Pages.GetPages();
            }
            else
            {
                return Pages.GetPages(int.Parse(siteid));
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Page Get(int id)
        {
            return Pages.GetPage(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Page Post([FromBody] Page Page)
        {
            if (ModelState.IsValid)
            {
                Page = Pages.AddPage(Page);
            }
            return Page;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Page Put(int id, [FromBody] Page Page)
        {
            if (ModelState.IsValid)
            {
                Page = Pages.UpdatePage(Page);
            }
            return Page;
        }

        // PUT api/<controller>/?siteid=x&parentid=y
        [HttpPut]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int siteid, int? parentid)
        {
            int order = 1;
            List<Page> pages = Pages.GetPages(siteid).ToList();
            foreach (Page page in pages.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
            {
                if (page.Order != order)
                {
                    page.Order = order;
                    Pages.UpdatePage(page);
                }
                order += 2;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Pages.DeletePage(id);
        }
    }
}
