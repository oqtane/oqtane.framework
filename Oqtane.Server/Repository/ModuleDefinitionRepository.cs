using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Extensions;
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
        private List<ModuleDefinition> _moduleDefinitions; // lazy load

        public ModuleDefinitionRepository(MasterDBContext context, IMemoryCache cache, IPermissionRepository permissions)
        {
            _db = context;
            _cache = cache;
            _permissions = permissions;
        }

        public IEnumerable<ModuleDefinition> GetModuleDefinitions()
        {
            return LoadModuleDefinitions(-1); // used only during startup
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
            _db.Entry(moduleDefinition).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(moduleDefinition.SiteId, EntityNames.ModuleDefinition, moduleDefinition.ModuleDefinitionId, moduleDefinition.Permissions);
            _cache.Remove("moduledefinitions:" + moduleDefinition.SiteId.ToString());
        }

        public void DeleteModuleDefinition(int moduleDefinitionId, int siteId)
        {
            ModuleDefinition moduleDefinition = _db.ModuleDefinition.Find(moduleDefinitionId);
            _permissions.DeletePermissions(siteId, EntityNames.ModuleDefinition, moduleDefinitionId);
            _db.ModuleDefinition.Remove(moduleDefinition);
            _db.SaveChanges();
        }

        public List<ModuleDefinition> LoadModuleDefinitions(int siteId)
        {
            // get module definitions for site
            List<ModuleDefinition> moduleDefinitions = _cache.GetOrCreate("moduledefinitions:" + siteId.ToString(), entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return LoadSiteModuleDefinitions(siteId);
            });
            return moduleDefinitions;
        }

        private List<ModuleDefinition> LoadSiteModuleDefinitions(int siteId)
        {
            if (_moduleDefinitions == null)
            {
                // get module assemblies 
                _moduleDefinitions = LoadModuleDefinitionsFromAssemblies();
            }

            List<ModuleDefinition> moduleDefinitions = _moduleDefinitions;

            List<Permission> permissions = new List<Permission>();
            if (siteId != -1)
            {
                // get module definition permissions for site
                permissions = _permissions.GetPermissions(siteId, EntityNames.ModuleDefinition).ToList();
            }

            // get module definitions in database
            List<ModuleDefinition> moduledefs = _db.ModuleDefinition.ToList();

            // sync module assemblies with database
            foreach (ModuleDefinition moduledefinition in moduleDefinitions)
            {
                ModuleDefinition moduledef = moduledefs.Where(item => item.ModuleDefinitionName == moduledefinition.ModuleDefinitionName).FirstOrDefault();
                if (moduledef == null)
                {
                    // new module definition
                    moduledef = new ModuleDefinition {ModuleDefinitionName = moduledefinition.ModuleDefinitionName};
                    _db.ModuleDefinition.Add(moduledef);
                    _db.SaveChanges();
                    if (siteId != -1)
                    {
                        _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledef.ModuleDefinitionId, moduledefinition.Permissions);
                    }
                }
                else
                {
                    // existing module definition
                    if (!string.IsNullOrEmpty(moduledef.Name))
                    {
                        moduledefinition.Name = moduledef.Name;
                    }

                    if (!string.IsNullOrEmpty(moduledef.Description))
                    {
                        moduledefinition.Description = moduledef.Description;
                    }

                    if (!string.IsNullOrEmpty(moduledef.Categories))
                    {
                        moduledefinition.Categories = moduledef.Categories;
                    }

                    if (!string.IsNullOrEmpty(moduledef.Version))
                    {
                        moduledefinition.Version = moduledef.Version;
                    }

                    if (siteId != -1)
                    {
                        if (permissions.Count == 0)
                        {
                            _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledef.ModuleDefinitionId, moduledefinition.Permissions);
                        }
                        else
                        {
                            if (permissions.Where(item => item.EntityId == moduledef.ModuleDefinitionId).Any())
                            {
                                moduledefinition.Permissions = permissions.Where(item => item.EntityId == moduledef.ModuleDefinitionId).EncodePermissions();
                            }
                            else
                            {
                                _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledef.ModuleDefinitionId, moduledefinition.Permissions);
                            }
                        }
                    }

                    // remove module definition from list as it is already synced
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
                if (siteId != -1)
                {
                    _permissions.DeletePermissions(siteId, EntityNames.ModuleDefinition, moduledefinition.ModuleDefinitionId);
                }

                _db.ModuleDefinition.Remove(moduledefinition); // delete
                _db.SaveChanges();
            }

            return moduleDefinitions;
        }

        private List<ModuleDefinition> LoadModuleDefinitionsFromAssemblies()
        {
            List<ModuleDefinition> moduleDefinitions = new List<ModuleDefinition>();
            // iterate through Oqtane module assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
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
                // Check if type should be ignored
                if (modulecontroltype.IsOqtaneIgnore()) continue;

                // create namespace root typename
                string qualifiedModuleType = modulecontroltype.Namespace + ", " + modulecontroltype.Assembly.GetName().Name;

                int index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == qualifiedModuleType);
                if (index == -1)
                {
                    // determine if this module implements IModule
                    Type moduletype = assembly
                        .GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace == modulecontroltype.Namespace || item.Namespace.StartsWith(modulecontroltype.Namespace + "."))
                        .FirstOrDefault(item => item.GetInterfaces().Contains(typeof(IModule)));
                    if (moduletype != null)
                    {
                        // get property values from IModule
                        var moduleobject = Activator.CreateInstance(moduletype) as IModule;
                        moduledefinition = moduleobject.ModuleDefinition;
                    }
                    else
                    {
                        // set default property values
                        moduledefinition = new ModuleDefinition
                        {
                            Name = Utilities.GetTypeNameLastSegment(modulecontroltype.Namespace, 0),
                            Description = "Manage " + Utilities.GetTypeNameLastSegment(modulecontroltype.Namespace, 0),
                            Categories = ((qualifiedModuleType.StartsWith("Oqtane.Modules.Admin.")) ? "Admin" : "")
                        };
                    }

                    // set internal properties
                    moduledefinition.ModuleDefinitionName = qualifiedModuleType;
                    moduledefinition.Version = ""; // will be populated from database
                    moduledefinition.ControlTypeTemplate = modulecontroltype.Namespace + "." + Constants.ActionToken + ", " + modulecontroltype.Assembly.GetName().Name;
                    moduledefinition.AssemblyName = assembly.GetName().Name;

                    if (string.IsNullOrEmpty(moduledefinition.Categories))
                    {
                        moduledefinition.Categories = "Common";
                    }

                    if (moduledefinition.Categories == "Admin")
                    {
                        moduledefinition.Permissions = new List<Permission>
                        {
                            new Permission(PermissionNames.Utilize, Constants.AdminRole, true)
                        }.EncodePermissions();
                    }
                    else
                    {
                        moduledefinition.Permissions = new List<Permission>
                        {
                            new Permission(PermissionNames.Utilize, Constants.AdminRole, true),
                            new Permission(PermissionNames.Utilize, Constants.RegisteredRole, true)
                        }.EncodePermissions();
                    }

                    Console.WriteLine($"Registering module: {moduledefinition.ModuleDefinitionName}");
                    moduledefinitions.Add(moduledefinition);
                    index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == qualifiedModuleType);
                }

                moduledefinition = moduledefinitions[index];
                // actions
                var modulecontrolobject = Activator.CreateInstance(modulecontroltype) as IModuleControl;
                string actions = modulecontrolobject.Actions;
                if (!string.IsNullOrEmpty(actions))
                {
                    foreach (string action in actions.Split(','))
                    {
                        moduledefinition.ControlTypeRoutes += (action + "=" + modulecontroltype.FullName + ", " + modulecontroltype.Assembly.GetName().Name + ";");
                    }
                }

                moduledefinitions[index] = moduledefinition;
            }

            return moduledefinitions;
        }
    }
}
