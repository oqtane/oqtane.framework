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
        private readonly IModuleRepository Modules;
        private readonly IPageModuleRepository PageModules;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public PageController(IPageRepository Pages, IModuleRepository Modules, IPageModuleRepository PageModules, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.Pages = Pages;
            this.Modules = Modules;
            this.PageModules = PageModules;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid)
        {
            List<Page> pages = new List<Page>();
            foreach (Page page in Pages.GetPages(int.Parse(siteid)))
            {
                if (UserPermissions.IsAuthorized(User, "View", page.Permissions))
                {
                    pages.Add(page);
                }
            }
            return pages;
        }

        // GET api/<controller>/5?userid=x
        [HttpGet("{id}")]
        public Page Get(int id, string userid)
        {
            Page page;
            if (string.IsNullOrEmpty(userid))
            {
                page = Pages.GetPage(id);
            }
            else
            {
                page = Pages.GetPage(id, int.Parse(userid));
            }
            if (UserPermissions.IsAuthorized(User, "View", page.Permissions))
            {
                return page;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Page {Page}", page);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post([FromBody] Page Page)
        {
            if (ModelState.IsValid)
            {
                string permissions;
                if (Page.ParentId != null)
                {
                    permissions = Pages.GetPage(Page.ParentId.Value).Permissions;
                }
                else
                {
                    permissions = UserSecurity.SetPermissionStrings(new List<PermissionString> { new PermissionString { PermissionName = "Edit", Permissions = Constants.AdminRole } });
                }
            
                if (UserPermissions.IsAuthorized(User, "Edit", permissions))
                {
                    Page = Pages.AddPage(Page);
                    logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", Page);
                }
                else
                {
                    logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Page {Page}", Page);
                    HttpContext.Response.StatusCode = 401;
                    Page = null;
                }
            }
            return Page;
        }

        // POST api/<controller>/5?userid=x
        [HttpPost("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post(int id, string userid)
        {
            Page page = null;
            Page parent = Pages.GetPage(id);
            if (parent != null && parent.IsPersonalizable && !string.IsNullOrEmpty(userid))
            {
                page = new Page();
                page.SiteId = parent.SiteId;
                page.Name = parent.Name;
                page.Path = parent.Path;
                page.ParentId = parent.PageId;
                page.Order = 0;
                page.IsNavigation = false;
                page.EditMode = false;
                page.ThemeType = parent.ThemeType;
                page.LayoutType = parent.LayoutType;
                page.Icon = parent.Icon;
                List<PermissionString> permissions = new List<PermissionString>();
                permissions.Add(new PermissionString { PermissionName = "View", Permissions = "[" + userid + "]" });
                permissions.Add(new PermissionString { PermissionName = "Edit", Permissions = "[" + userid + "]" });
                page.Permissions = UserSecurity.SetPermissionStrings(permissions);
                page.IsPersonalizable = false;
                page.UserId = int.Parse(userid);
                page = Pages.AddPage(page);

                // copy modules
                List<PageModule> pagemodules = PageModules.GetPageModules(page.SiteId).ToList();
                foreach (PageModule pm in pagemodules.Where(item => item.PageId == parent.PageId && !item.IsDeleted))
                {
                    Module module = new Module();
                    module.SiteId = page.SiteId;
                    module.PageId = page.PageId;
                    module.ModuleDefinitionName = pm.Module.ModuleDefinitionName;
                    permissions = new List<PermissionString>();
                    permissions.Add(new PermissionString { PermissionName = "View", Permissions = "[" + userid + "]" });
                    permissions.Add(new PermissionString { PermissionName = "Edit", Permissions = "[" + userid + "]" });
                    module.Permissions = UserSecurity.SetPermissionStrings(permissions);
                    module = Modules.AddModule(module);

                    string content = Modules.ExportModule(pm.ModuleId);
                    if (content != "")
                    {
                        Modules.ImportModule(module.ModuleId, content);
                    }

                    PageModule pagemodule = new PageModule();
                    pagemodule.PageId = page.PageId;
                    pagemodule.ModuleId = module.ModuleId;
                    pagemodule.Title = pm.Title;
                    pagemodule.Pane = pm.Pane;
                    pagemodule.Order = pm.Order;
                    pagemodule.ContainerType = pm.ContainerType;

                    PageModules.AddPageModule(pagemodule);
                }
            }
            return page;
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page {Page}", Page);
                HttpContext.Response.StatusCode = 401;
                Page = null;
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page Order {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
                HttpContext.Response.StatusCode = 401;
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Page {PageId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }
    }
}
