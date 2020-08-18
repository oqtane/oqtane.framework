using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IPageRepository _pages;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public ModuleController(IModuleRepository modules, IPageModuleRepository pageModules, IPageRepository pages, IModuleDefinitionRepository moduleDefinitions, IUserPermissions userPermissions, ILogManager logger)
        {
            _modules = modules;
            _pageModules = pageModules;
            _pages = pages;
            _moduleDefinitions = moduleDefinitions;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Module> Get(string siteid)
        {
            List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(int.Parse(siteid)).ToList();
            List<Module> modules = new List<Module>();
            foreach (PageModule pagemodule in _pageModules.GetPageModules(int.Parse(siteid)))
            {
                if (_userPermissions.IsAuthorized(User,PermissionNames.View, pagemodule.Module.Permissions))
                {
                    Module module = new Module();
                    module.SiteId = pagemodule.Module.SiteId;
                    module.ModuleDefinitionName = pagemodule.Module.ModuleDefinitionName;
                    module.AllPages = pagemodule.Module.AllPages;
                    module.Permissions = pagemodule.Module.Permissions;
                    module.CreatedBy = pagemodule.Module.CreatedBy;
                    module.CreatedOn = pagemodule.Module.CreatedOn;
                    module.ModifiedBy = pagemodule.Module.ModifiedBy;
                    module.ModifiedOn = pagemodule.Module.ModifiedOn;
                    module.IsDeleted = pagemodule.IsDeleted;

                    module.PageModuleId = pagemodule.PageModuleId;
                    module.ModuleId = pagemodule.ModuleId;
                    module.PageId = pagemodule.PageId;
                    module.Title = pagemodule.Title;
                    module.Pane = pagemodule.Pane;
                    module.Order = pagemodule.Order;
                    module.ContainerType = pagemodule.ContainerType;

                    module.ModuleDefinition = moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName);

                    modules.Add(module);
                }
            }
            return modules;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Module Get(int id)
        {
            Module module = _modules.GetModule(id);
            if (_userPermissions.IsAuthorized(User,PermissionNames.View, module.Permissions))
            {
                List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                module.ModuleDefinition = moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName);
                return module;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Module {Module}", module);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Module Post([FromBody] Module module)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Page, module.PageId, PermissionNames.Edit))
            {
                module = _modules.AddModule(module);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Added {Module}", module);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Module {Module}", module);
                HttpContext.Response.StatusCode = 401;
                module = null;
            }
            return module;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Module Put(int id, [FromBody] Module module)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Module, module.ModuleId, PermissionNames.Edit))
            {
                module = _modules.UpdateModule(module);
                if (module.AllPages)
                {
                    var pageModule = _pageModules.GetPageModules(module.SiteId).FirstOrDefault(item => item.ModuleId == module.ModuleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Updated {Module}", module);

                    var pages = _pages.GetPages(module.SiteId).ToList();
                    foreach (Page page in pages)
                    {
                        if (page.PageId != pageModule.PageId && !page.Path.StartsWith("admin/"))
                        {
                            _pageModules.AddPageModule(new PageModule { PageId = page.PageId, ModuleId = pageModule.ModuleId, Title = pageModule.Title, Pane = pageModule.Pane, Order = pageModule.Order, ContainerType = pageModule.ContainerType });
                        }
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Module {Module}", module);
                HttpContext.Response.StatusCode = 401;
                module = null;
            }
            return module;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            if (_userPermissions.IsAuthorized(User, EntityNames.Module, id, PermissionNames.Edit))
            {
                _modules.DeleteModule(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Module {ModuleId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // GET api/<controller>/export?moduleid=x
        [HttpGet("export")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public string Export(int moduleid)
        {
            string content = "";
            if (_userPermissions.IsAuthorized(User, EntityNames.Module, moduleid, PermissionNames.Edit))
            {
                content = _modules.ExportModule(moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "User Not Authorized To Export Module {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = 401;
            }
            return content;
        }

        // POST api/<controller>/import?moduleid=x
        [HttpPost("import")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public bool Import(int moduleid, [FromBody] string content)
        {
            bool success = false;
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Module, moduleid, PermissionNames.Edit))
            {
                success = _modules.ImportModule(moduleid, content);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "User Not Authorized To Import Module {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = 401;
            }
            return success;
        }
    }
}
