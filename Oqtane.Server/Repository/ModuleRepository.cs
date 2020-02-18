using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Reflection;
using System;
using Oqtane.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Oqtane.Repository
{
    public class ModuleRepository : IModuleRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;
        private readonly IModuleDefinitionRepository ModuleDefinitions;
        private readonly IServiceProvider ServiceProvider;

        public ModuleRepository(TenantDBContext context, IPermissionRepository Permissions, IModuleDefinitionRepository ModuleDefinitions, IServiceProvider ServiceProvider)
        {
            db = context;
            this.Permissions = Permissions;
            this.ModuleDefinitions = ModuleDefinitions;
            this.ServiceProvider = ServiceProvider;
        }

        public IEnumerable<Models.Module> GetModules()
        {
            return db.Module;
        }

        public Models.Module AddModule(Models.Module Module)
        {
            db.Module.Add(Module);
            db.SaveChanges();
            Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Models.Module UpdateModule(Models.Module Module)
        {
            db.Entry(Module).State = EntityState.Modified;
            db.SaveChanges();
            Permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Models.Module GetModule(int ModuleId)
        {
            Models.Module module = db.Module.Find(ModuleId);
            if (module != null)
            {
                List<Permission> permissions = Permissions.GetPermissions("Module", module.ModuleId).ToList();
                module.Permissions = Permissions.EncodePermissions(module.ModuleId, permissions);
            }
            return module;
        }

        public void DeleteModule(int ModuleId)
        {
            Models.Module Module = db.Module.Find(ModuleId);
            Permissions.DeletePermissions(Module.SiteId, "Module", ModuleId);
            db.Module.Remove(Module);
            db.SaveChanges();
        }

        public string ExportModule(int ModuleId)
        {
            string content = "";
            try
            {
                Models.Module module = GetModule(ModuleId);
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

        public bool ImportModule(int ModuleId, string Content)
        {
            bool success = false;
            try
            {
                Models.Module module = GetModule(ModuleId);
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
            return success;
        }

    }
}
