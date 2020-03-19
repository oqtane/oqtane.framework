using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class ModuleDefinitionRepository : IModuleDefinitionRepository
    {
        private MasterDBContext _db;
        private readonly IMemoryCache _cache;
        private readonly IPermissionRepository _permissions;

        public ModuleDefinitionRepository(MasterDBContext context, IMemoryCache cache, IPermissionRepository permissions)
        {
            _db = context;
            _cache = cache;
            _permissions = permissions;
        }

        public IEnumerable<ModuleDefinition> GetModuleDefinitions(int siteId)
        {
            return LoadModuleDefinitions(siteId);
        }

        public ModuleDefinition GetModuleDefinition(int moduleDefinitionId, int siteId)
        {
            List<ModuleDefinition> moduledefinitions = LoadModuleDefinitions(siteId);
            return moduledefinitions.Find(item => item.ModuleDefinitionId == moduleDefinitionId);
        }

        public void UpdateModuleDefinition(ModuleDefinition moduleDefinition)
        {
            _permissions.UpdatePermissions(moduleDefinition.SiteId, EntityNames.ModuleDefinition, moduleDefinition.ModuleDefinitionId, moduleDefinition.Permissions);
            _cache.Remove("moduledefinitions");
        }

        public void DeleteModuleDefinition(int moduleDefinitionId, int siteId)
        {
            ModuleDefinition moduleDefinition = _db.ModuleDefinition.Find(moduleDefinitionId);
            _permissions.DeletePermissions(siteId, EntityNames.ModuleDefinition, moduleDefinitionId);
            _db.ModuleDefinition.Remove(moduleDefinition);
            _db.SaveChanges();
            _cache.Remove("moduledefinitions");
        }

        public List<ModuleDefinition> LoadModuleDefinitions(int siteId)
        {
            List<ModuleDefinition> moduleDefinitions;

            // get run-time module definitions 
            moduleDefinitions = _cache.GetOrCreate("moduledefinitions", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return LoadModuleDefinitionsFromAssemblies();
            });

            // get module defintion permissions for site
            List<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.ModuleDefinition).ToList();

            // get module definitions in database
            List<ModuleDefinition> moduledefs = _db.ModuleDefinition.ToList();

            // sync run-time module definitions with database
            foreach (ModuleDefinition moduledefinition in moduleDefinitions)
            {
                ModuleDefinition moduledef = moduledefs.Where(item => item.ModuleDefinitionName == moduledefinition.ModuleDefinitionName).FirstOrDefault();
                if (moduledef == null)
                {
                    // new module definition
                    moduledef = new ModuleDefinition { ModuleDefinitionName = moduledefinition.ModuleDefinitionName };
                    _db.ModuleDefinition.Add(moduledef);
                    _db.SaveChanges();
                    _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledef.ModuleDefinitionId, moduledefinition.Permissions);
                }
                else
                {
                    // existing module definition
                    if (permissions.Count == 0)
                    {
                        _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledef.ModuleDefinitionId, moduledefinition.Permissions);
                    }
                    else
                    {
                        moduledefinition.Permissions = _permissions.EncodePermissions(permissions.Where(item => item.EntityId == moduledef.ModuleDefinitionId));
                    }
                    // remove module definition from list
                    moduledefs.Remove(moduledef);  
                }
                moduledefinition.ModuleDefinitionId = moduledef.ModuleDefinitionId;
                moduledefinition.SiteId = siteId;
                moduledefinition.CreatedBy = moduledef.CreatedBy;
                moduledefinition.CreatedOn = moduledef.CreatedOn;
                moduledefinition.ModifiedBy = moduledef.ModifiedBy;
                moduledefinition.ModifiedOn = moduledef.ModifiedOn;
            }

            // any remaining module definitions are orphans
            foreach (ModuleDefinition moduledefinition in moduledefs)
            {
                _permissions.DeletePermissions(siteId, EntityNames.ModuleDefinition, moduledefinition.ModuleDefinitionId);
                _db.ModuleDefinition.Remove(moduledefinition); // delete
            }

            return moduleDefinitions;
        }

        private List<ModuleDefinition> LoadModuleDefinitionsFromAssemblies()
        {
            List<ModuleDefinition> moduleDefinitions = new List<ModuleDefinition>();
            // iterate through Oqtane module assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                moduleDefinitions = LoadModuleDefinitionsFromAssembly(moduleDefinitions, assembly);
            }
            return moduleDefinitions;
        }

        private List<ModuleDefinition> LoadModuleDefinitionsFromAssembly(List<ModuleDefinition> moduledefinitions, Assembly assembly)
        {
            ModuleDefinition moduledefinition;
            Type[] modulecontroltypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IModuleControl))).ToArray();
            foreach (Type modulecontroltype in modulecontroltypes)
            {
                if (modulecontroltype.Name != "ModuleBase" && !modulecontroltype.Namespace.EndsWith(".Controls"))
                {
                    string[] typename = modulecontroltype.AssemblyQualifiedName.Split(',').Select(item => item.Trim()).ToList().ToArray();
                    string[] segments = typename[0].Split('.');
                    Array.Resize(ref segments, segments.Length - 1);
                    string moduleType = string.Join(".", segments);
                    string qualifiedModuleType = moduleType + ", " + typename[1];

                    int index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == qualifiedModuleType);
                    if (index == -1)
                    {
                        // determine if this module implements IModule
                        Type moduletype = assembly
                            .GetTypes()
                            .Where(item => item.Namespace != null)
                            .Where(item => item.Namespace.StartsWith(moduleType))
                            .FirstOrDefault(item => item.GetInterfaces().Contains(typeof(IModule)));
                        if (moduletype != null)
                        {
                            var moduleobject = Activator.CreateInstance(moduletype);
                            Dictionary<string, string> properties = (Dictionary<string, string>)moduletype.GetProperty("Properties").GetValue(moduleobject);
                            moduledefinition = new ModuleDefinition
                            {
                                ModuleDefinitionName = qualifiedModuleType,
                                Name = GetProperty(properties, "Name"),
                                Description = GetProperty(properties, "Description"),
                                Categories = GetProperty(properties, "Categories"),
                                Version = GetProperty(properties, "Version"),
                                Owner = GetProperty(properties, "Owner"),
                                Url = GetProperty(properties, "Url"),
                                Contact = GetProperty(properties, "Contact"),
                                License = GetProperty(properties, "License"),
                                Dependencies = GetProperty(properties, "Dependencies"),
                                PermissionNames = GetProperty(properties, "PermissionNames"),
                                ServerAssemblyName = GetProperty(properties, "ServerAssemblyName"),
                                ControlTypeTemplate = moduleType + "." + Constants.ActionToken + ", " + typename[1],
                                ControlTypeRoutes = "",
                                AssemblyName = assembly.FullName.Split(",")[0],
                                Permissions = ""
                            };
                        }
                        else
                        {
                            moduledefinition = new ModuleDefinition
                            {
                                ModuleDefinitionName = qualifiedModuleType,
                                Name = moduleType.Substring(moduleType.LastIndexOf(".") + 1),
                                Description = moduleType.Substring(moduleType.LastIndexOf(".") + 1),
                                Categories = ((qualifiedModuleType.StartsWith("Oqtane.Modules.Admin.")) ? "Admin" : ""),
                                Version = new Version(1, 0, 0).ToString(),
                                Owner = "",
                                Url = "",
                                Contact = "",
                                License = "",
                                Dependencies = "",
                                PermissionNames = "",
                                ServerAssemblyName = "",
                                ControlTypeTemplate = moduleType + "." + Constants.ActionToken + ", " + typename[1],
                                ControlTypeRoutes = "",
                                AssemblyName = assembly.FullName.Split(",")[0],
                                Permissions = ""
                            };
                        }
                        // permissions
                        if (moduledefinition.Categories == "Admin")
                        {
                            moduledefinition.Permissions = "[{\"PermissionName\":\"Utilize\",\"Permissions\":\"" + Constants.AdminRole + "\"}]";
                        }
                        else
                        {
                            moduledefinition.Permissions = "[{\"PermissionName\":\"Utilize\",\"Permissions\":\"" + Constants.AdminRole + ";" + Constants.RegisteredRole + "\"}]";
                        }
                        moduledefinitions.Add(moduledefinition);
                        index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == qualifiedModuleType);
                    }
                    moduledefinition = moduledefinitions[index];
                    // actions
                    var modulecontrolobject = Activator.CreateInstance(modulecontroltype);
                    string actions = (string)modulecontroltype.GetProperty("Actions").GetValue(modulecontrolobject);
                    if (actions != "")
                    {
                        foreach (string action in actions.Split(','))
                        {
                            moduledefinition.ControlTypeRoutes += (action + "=" + modulecontroltype.FullName + ", " + typename[1] + ";");
                        }
                    }
                    moduledefinitions[index] = moduledefinition;
                }
            }

            return moduledefinitions;
        }

        private string GetProperty(Dictionary<string, string> properties, string key)
        {
            string value = "";
            if (properties.ContainsKey(key))
            {
                value = properties[key];
            }
            return value;
        }
    }
}
