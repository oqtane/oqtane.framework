using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Module = Oqtane.Models.Module;

namespace Oqtane.Repository
{
    public class ModuleRepository : IModuleRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;
        private readonly ISettingRepository _settings;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IServiceProvider _serviceProvider;

        public ModuleRepository(TenantDBContext context, IPermissionRepository permissions, ISettingRepository settings, IModuleDefinitionRepository moduleDefinitions, IServiceProvider serviceProvider)
        {
            _db = context;
            _permissions = permissions;
            _settings = settings;
            _moduleDefinitions = moduleDefinitions;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Module> GetModules(int siteId)
        {
            return _db.Module.Where(item => item.SiteId == siteId).ToList();
        }

        public Module AddModule(Module module)
        {
            _db.Module.Add(module);
            _db.SaveChanges();
            _permissions.UpdatePermissions(module.SiteId, EntityNames.Module, module.ModuleId, module.PermissionList);
            return module;
        }

        public Module UpdateModule(Module module)
        {
            _db.Entry(module).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(module.SiteId, EntityNames.Module, module.ModuleId, module.PermissionList);
            return module;
        }

        public Module GetModule(int moduleId)
        {
            return GetModule(moduleId, true);
        }

        public Module GetModule(int moduleId, bool tracking)
        {
            Module module;
            if (tracking)
            {
                module = _db.Module.Find(moduleId);
            }
            else
            {
                module = _db.Module.AsNoTracking().FirstOrDefault(item => item.ModuleId == moduleId);
            }
            if (module != null)
            {
                module.PermissionList = _permissions.GetPermissions(module.SiteId, EntityNames.Module, module.ModuleId)?.ToList();
            }
            return module;
        }

        public void DeleteModule(int moduleId)
        {
            Module module = _db.Module.Find(moduleId);
            _permissions.DeletePermissions(module.SiteId, EntityNames.Module, moduleId);
            _settings.DeleteSettings(EntityNames.Module, moduleId);
            _db.Module.Remove(module);
            _db.SaveChanges();
        }

        public string ExportModule(int moduleId)
        {
            string content = "";
            try
            {
                Module module = GetModule(moduleId);
                if (module != null)
                {
                    List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                    ModuleDefinition moduledefinition = moduledefinitions.FirstOrDefault(item => item.ModuleDefinitionName == module.ModuleDefinitionName);
                    if (moduledefinition != null)
                    {
                        ModuleContent modulecontent = new ModuleContent();
                        modulecontent.ModuleDefinitionName = moduledefinition.ModuleDefinitionName;
                        modulecontent.Version = moduledefinition.Version;
                        modulecontent.Content = "";

                        if (moduledefinition.ServerManagerType != "")
                        {
                            Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                            if (moduletype != null && moduletype.GetInterface("IPortable") != null)
                            {
                                try
                                {
                                    module.Settings = _settings.GetSettings(EntityNames.Module, moduleId).ToDictionary(x => x.SettingName, x => x.SettingValue);
                                    var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                    modulecontent.Content = ((IPortable)moduleobject).ExportModule(module);
                                }
                                catch
                                {
                                    // error in IPortable implementation
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

        public bool ImportModule(int moduleId, string content)
        {
            bool success = false;
            try
            {
                Module module = GetModule(moduleId);
                if (module != null)
                {
                    List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                    ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == module.ModuleDefinitionName).FirstOrDefault();
                    if (moduledefinition != null)
                    {
                        ModuleContent modulecontent = JsonSerializer.Deserialize<ModuleContent>(content.Replace("\n", ""));
                        if (modulecontent.ModuleDefinitionName == moduledefinition.ModuleDefinitionName)
                        {
                            if (moduledefinition.ServerManagerType != "")
                            {
                                Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                                if (moduletype != null && moduletype.GetInterface("IPortable") != null)
                                {
                                    try
                                    {
                                        module.Settings = _settings.GetSettings(EntityNames.Module, moduleId).ToDictionary(x => x.SettingName, x => x.SettingValue);
                                        var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                        ((IPortable)moduleobject).ImportModule(module, modulecontent.Content, modulecontent.Version);
                                        success = true;
                                    }
                                    catch 
                                    {
                                        // error in IPortable implementation
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
