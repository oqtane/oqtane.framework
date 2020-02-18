using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System.Reflection;
using Oqtane.Infrastructure;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly IModuleRepository Modules;
        private readonly IPageModuleRepository PageModules;
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public ModuleController(IModuleRepository Modules, IPageModuleRepository PageModules, IModuleDefinitionRepository ModuleDefinitions, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.Modules = Modules;
            this.PageModules = PageModules;
            this.ModuleDefinitions = ModuleDefinitions;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Models.Module> Get(string siteid)
        {
            List<ModuleDefinition> moduledefinitions = ModuleDefinitions.GetModuleDefinitions(int.Parse(siteid)).ToList();
            List<Models.Module> modules = new List<Models.Module>();
            foreach (PageModule pagemodule in PageModules.GetPageModules(int.Parse(siteid)))
            {
                if (UserPermissions.IsAuthorized(User, "View", pagemodule.Module.Permissions))
                {
                    Models.Module module = new Models.Module();
                    module.SiteId = pagemodule.Module.SiteId;
                    module.ModuleDefinitionName = pagemodule.Module.ModuleDefinitionName;
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
        public Models.Module Get(int id)
        {
            Models.Module module = Modules.GetModule(id);
            if (UserPermissions.IsAuthorized(User, "View", module.Permissions))
            {
                List<ModuleDefinition> moduledefinitions = ModuleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                module.ModuleDefinition = moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName);
                return module;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Module {Module}", module);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Models.Module Post([FromBody] Models.Module Module)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Page", Module.PageId, "Edit"))
            {
                Module = Modules.AddModule(Module);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Added {Module}", Module);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Module {Module}", Module);
                HttpContext.Response.StatusCode = 401;
                Module = null;
            }
            return Module;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Models.Module Put(int id, [FromBody] Models.Module Module)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Module", Module.ModuleId, "Edit"))
            {
                Module = Modules.UpdateModule(Module);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Updated {Module}", Module);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Module {Module}", Module);
                HttpContext.Response.StatusCode = 401;
                Module = null;
            }
            return Module;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            if (UserPermissions.IsAuthorized(User, "Module", id, "Edit"))
            {
                Modules.DeleteModule(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleId}", id);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Module {ModuleId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // GET api/<controller>/export?moduleid=x
        [HttpGet("export")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public string Export(int moduleid)
        {
            string content = "";
            if (UserPermissions.IsAuthorized(User, "Module", moduleid, "Edit"))
            {
                content = Modules.ExportModule(moduleid);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Other, "User Not Authorized To Export Module {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = 401;
            }
            return content;
        }

        // POST api/<controller>/import?moduleid=x
        [HttpPost("import")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public bool Import(int moduleid, [FromBody] string Content)
        {
            bool success = false;
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Module", moduleid, "Edit"))
            {
                success = Modules.ImportModule(moduleid, Content);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Other, "User Not Authorized To Import Module {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = 401;
            }
            return success;
        }
    }
}
