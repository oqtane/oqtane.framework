using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System.Reflection;
using System;
using Oqtane.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly IModuleRepository Modules;
        private readonly IPageModuleRepository PageModules;
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IServiceProvider ServiceProvider;

        public ModuleController(IModuleRepository Modules, IPageModuleRepository PageModules, IModuleDefinitionRepository ModuleDefinitions, IServiceProvider ServiceProvider)
        {
            this.Modules = Modules;
            this.PageModules = PageModules;
            this.ModuleDefinitions = ModuleDefinitions;
            this.ServiceProvider = ServiceProvider;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Models.Module> Get(string siteid)
        {
            List<Models.Module> modulelist = new List<Models.Module>();
            foreach (PageModule pagemodule in PageModules.GetPageModules(int.Parse(siteid)))
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
                modulelist.Add(module);
            }
            return modulelist;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Models.Module Get(int id)
        {
            return Modules.GetModule(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Models.Module Post([FromBody] Models.Module Module)
        {
            if (ModelState.IsValid)
            {
                Module = Modules.AddModule(Module);
            }
            return Module;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Models.Module Put(int id, [FromBody] Models.Module Module)
        {
            if (ModelState.IsValid)
            {
                Module = Modules.UpdateModule(Module);
            }
            return Module;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Modules.DeleteModule(id);
        }

        // GET api/<controller>/export?moduleid=x
        [HttpGet("export")]
        [Authorize(Roles = Constants.AdminRole)]
        public string Export(int moduleid)
        {
            string content = "";
            try
            {
                Models.Module module = Modules.GetModule(moduleid);
                if (module != null)
                {
                    List<ModuleDefinition> moduledefinitions = ModuleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                    ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == module.ModuleDefinitionName).FirstOrDefault();
                    if (moduledefinition != null)
                    {
                        ModuleContent modulecontent = new ModuleContent();
                        modulecontent.ModuleDefinitionName = moduledefinition.ModuleDefinitionName;
                        modulecontent.Version = moduledefinition.Version;
                        modulecontent.Content = "";

                        if (moduledefinition.ServerAssemblyName != "")
                        {
                            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(item => item.FullName.StartsWith(moduledefinition.ServerAssemblyName)).FirstOrDefault();
                            if (assembly != null)
                            {
                                Type moduletype = assembly.GetTypes()
                                    .Where(item => item.Namespace != null)
                                    .Where(item => item.Namespace.StartsWith(moduledefinition.ModuleDefinitionName.Substring(0, moduledefinition.ModuleDefinitionName.IndexOf(","))))
                                    .Where(item => item.GetInterfaces().Contains(typeof(IPortable))).FirstOrDefault();
                                if (moduletype != null)
                                {
                                    var moduleobject = ActivatorUtilities.CreateInstance(ServiceProvider, moduletype);
                                    modulecontent.Content = ((IPortable)moduleobject).ExportModule(module);
                                }
                            }
                        }
                        content = JsonSerializer.Serialize(modulecontent);
                    }
                }
            }
            catch
            {
                // error occurred during export
            }
            return content;
        }

        // POST api/<controller>/import?moduleid=x
        [HttpPost("import")]
        [Authorize(Roles = Constants.AdminRole)]
        public bool Import(int moduleid, [FromBody] string Content)
        {
            bool success = false;
            if (ModelState.IsValid)
            {
                try
                {
                    Models.Module module = Modules.GetModule(moduleid);
                    if (module != null)
                    {
                        List<ModuleDefinition> moduledefinitions = ModuleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                        ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == module.ModuleDefinitionName).FirstOrDefault();
                        if (moduledefinition != null)
                        {
                            ModuleContent modulecontent = JsonSerializer.Deserialize<ModuleContent>(Content);
                            if (modulecontent.ModuleDefinitionName == moduledefinition.ModuleDefinitionName)
                            {
                                if (moduledefinition.ServerAssemblyName != "")
                                {
                                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                                        .Where(item => item.FullName.StartsWith(moduledefinition.ServerAssemblyName)).FirstOrDefault();
                                    if (assembly != null)
                                    {
                                        Type moduletype = assembly.GetTypes()
                                            .Where(item => item.Namespace != null)
                                            .Where(item => item.Namespace.StartsWith(moduledefinition.ModuleDefinitionName.Substring(0, moduledefinition.ModuleDefinitionName.IndexOf(","))))
                                            .Where(item => item.GetInterfaces().Contains(typeof(IPortable))).FirstOrDefault();
                                        if (moduletype != null)
                                        {
                                            var moduleobject = ActivatorUtilities.CreateInstance(ServiceProvider, moduletype);
                                            ((IPortable)moduleobject).ImportModule(module, modulecontent.Content, modulecontent.Version);
                                            success = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // error occurred during import
                }
            }
            return success;
        }
    }
}
