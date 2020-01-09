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
        private readonly IPageRepository _pages;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public PageController(IPageRepository pages, IUserPermissions userPermissions, ILogManager logger)
        {
            _pages = pages;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid)
        {
            if (siteid == "")
            {
                return _pages.GetAll();
            }
            else
            {
                return _pages.GetAll(int.Parse(siteid));
            }
        }

        // GET api/<controller>/5?userid=x
        [HttpGet("{id}")]
        public Page Get(int id, string userid)
        {
            if (string.IsNullOrEmpty(userid))
            {
                return _pages.Get(id);
            }
            else
            {
                return _pages.Get(id, int.Parse(userid));
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post([FromBody] Page page)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, "Edit", page.Permissions))
            {
                page = _pages.Add(page);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", page);
            }
            return page;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Put(int id, [FromBody] Page page)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, "Page", page.PageId, "Edit"))
            {
                page = _pages.Update(page);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Updated {Page}", page);
            }
            return page;
        }

        // PUT api/<controller>/?siteid=x&pageid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int siteid, int pageid, int? parentid)
        {
            if (_userPermissions.IsAuthorized(User, "Page", pageid, "Edit"))
            {
                int order = 1;
                List<Page> pages = _pages.GetAll(siteid).ToList();
                foreach (Page page in pages.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
                {
                    if (page.Order != order)
                    {
                        page.Order = order;
                        _pages.Update(page);
                    }
                    order += 2;
                }
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Order Updated {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            if (_userPermissions.IsAuthorized(User, "Page", id, "Edit"))
            {
                _pages.Delete(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Deleted {PageId}", id);
            }
        }
    }
}
