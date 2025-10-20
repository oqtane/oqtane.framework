using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Module = Oqtane.Models.Module;

namespace Oqtane.Repository
{
    public interface IModuleRepository
    {
        IEnumerable<Module> GetModules(int siteId);
        Module AddModule(Module module);
        Module UpdateModule(Module module);
        Module GetModule(int moduleId);
        Module GetModule(int moduleId, bool tracking);
        void DeleteModule(int moduleId);
        string ExportModule(int moduleId);
        bool ImportModule(int moduleId, string content);
    }

    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IPermissionRepository _permissions;
        private readonly ISettingRepository _settings;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IServiceProvider _serviceProvider;

        public ModuleRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IPermissionRepository permissions, ISettingRepository settings, IModuleDefinitionRepository moduleDefinitions, IServiceProvider serviceProvider)
        {
            _dbContextFactory = dbContextFactory;
            _permissions = permissions;
            _settings = settings;
            _moduleDefinitions = moduleDefinitions;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Module> GetModules(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Module.Where(item => item.SiteId == siteId).ToList();
        }

        public Module AddModule(Module module)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Module.Add(module);
            db.SaveChanges();
            _permissions.UpdatePermissions(module.SiteId, EntityNames.Module, module.ModuleId, module.PermissionList);
            return module;
        }

        public Module UpdateModule(Module module)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(module).State = EntityState.Modified;
            db.SaveChanges();
            _permissions.UpdatePermissions(module.SiteId, EntityNames.Module, module.ModuleId, module.PermissionList);
            return module;
        }

        public Module GetModule(int moduleId)
        {
            return GetModule(moduleId, true);
        }

        public Module GetModule(int moduleId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            Module module;
            if (tracking)
            {
                module = db.Module.Find(moduleId);
            }
            else
            {
                module = db.Module.AsNoTracking().FirstOrDefault(item => item.ModuleId == moduleId);
            }
            if (module != null)
            {
                module.PermissionList = _permissions.GetPermissions(module.SiteId, EntityNames.Module, module.ModuleId)?.ToList();
            }
            return module;
        }

        public void DeleteModule(int moduleId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var module = db.Module.Find(moduleId);
            _permissions.DeletePermissions(module.SiteId, EntityNames.Module, moduleId);
            _settings.DeleteSettings(EntityNames.Module, moduleId);
            db.Module.Remove(module);
            db.SaveChanges();
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
                        var settings = _settings.GetSettings(EntityNames.Module, moduleId);

                        ModuleContent modulecontent = new ModuleContent();
                        modulecontent.ModuleDefinitionName = moduledefinition.ModuleDefinitionName;
                        modulecontent.Version = moduledefinition.Version;
                        modulecontent.Settings = settings.Where(item => !item.IsPrivate).ToDictionary(x => x.SettingName, x => x.SettingValue);
                        modulecontent.Content = "";

                        if (moduledefinition.ServerManagerType != "")
                        {
                            Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                            if (moduletype != null && moduletype.GetInterface(nameof(IPortable)) != null)
                            {
                                try
                                {
                                    module.Settings = settings.ToDictionary(x => x.SettingName, x => x.SettingValue);
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
                            var settings = _settings.GetSettings(EntityNames.Module, moduleId);

                            if (modulecontent.Settings != null)
                            {
                                foreach (var kvp in modulecontent.Settings)
                                {
                                    var setting = settings.FirstOrDefault(item => item.SettingName == kvp.Key);
                                    if (setting == null)
                                    {
                                        setting = new Setting { EntityName = EntityNames.Module, EntityId = moduleId, SettingName = kvp.Key, SettingValue = kvp.Value, IsPrivate = false };
                                        _settings.AddSetting(setting);
                                    }
                                    else
                                    {
                                        if (setting.SettingValue != kvp.Value)
                                        {
                                            setting.SettingValue = kvp.Value;
                                            _settings.UpdateSetting(setting);
                                        }
                                    }
                                }
                            }

                            if (moduledefinition.ServerManagerType != "")
                            {
                                Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                                if (moduletype != null && moduletype.GetInterface(nameof(IPortable)) != null)
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
