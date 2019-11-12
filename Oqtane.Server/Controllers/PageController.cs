using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Infrastructure;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageController : Controller
    {
        private readonly IPageRepository Pages;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public PageController(IPageRepository Pages, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.Pages = Pages;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
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

        // GET api/<controller>/5?userid=x
        [HttpGet("{id}")]
        public Page Get(int id, string userid)
        {
            if (userid == "")
            {
                return Pages.GetPage(id);
            }
            else
            {
                return Pages.GetPage(id, int.Parse(userid));
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post([FromBody] Page Page)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Edit", Page.Permissions))
            {
                Page = Pages.AddPage(Page);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", Page);
            }
            return Page;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Put(int id, [FromBody] Page Page)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Page", Page.PageId, "Edit"))
            {
                Page = Pages.UpdatePage(Page);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Updated {Page}", Page);
            }
            return Page;
        }

        // PUT api/<controller>/?siteid=x&pageid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int siteid, int pageid, int? parentid)
        {
            if (UserPermissions.IsAuthorized(User, "Page", pageid, "Edit"))
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
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Order Updated {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            if (UserPermissions.IsAuthorized(User, "Page", id, "Edit"))
            {
                Pages.DeletePage(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Deleted {PageId}", id);
            }
        }
    }
}
