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
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IServiceProvider _serviceProvider;

        public ModuleRepository(TenantDBContext context, IPermissionRepository Permissions, IModuleDefinitionRepository ModuleDefinitions, IServiceProvider ServiceProvider)
        {
            _db = context;
            this._permissions = Permissions;
            this._moduleDefinitions = ModuleDefinitions;
            this._serviceProvider = ServiceProvider;
        }

        public IEnumerable<Models.Module> GetModules(int SiteId)
        {
            return _db.Module.Where(item => item.SiteId == SiteId).ToList();
        }

        public Models.Module AddModule(Models.Module Module)
        {
            _db.Module.Add(Module);
            _db.SaveChanges();
            _permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Models.Module UpdateModule(Models.Module Module)
        {
            _db.Entry(Module).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(Module.SiteId, "Module", Module.ModuleId, Module.Permissions);
            return Module;
        }

        public Models.Module GetModule(int ModuleId)
        {
            Models.Module module = _db.Module.Find(ModuleId);
            if (module != null)
            {
                List<Permission> permissions = _permissions.GetPermissions("Module", module.ModuleId).ToList();
                module.Permissions = _permissions.EncodePermissions(module.ModuleId, permissions);
            }
            return module;
        }

        public void DeleteModule(int ModuleId)
        {
            Models.Module Module = _db.Module.Find(ModuleId);
            _permissions.DeletePermissions(Module.SiteId, "Module", ModuleId);
            _db.Module.Remove(Module);
            _db.SaveChanges();
        }

        public string ExportModule(int ModuleId)
        {
            string content = "";
            try
            {
                Models.Module module = GetModule(ModuleId);
                if (module != null)
                {
                    List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
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
                                    var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
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
                    List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
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
                                        var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
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
