using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        private readonly ISettingRepository _settings;

        public ModuleDefinitionRepository(MasterDBContext context, IMemoryCache cache, IPermissionRepository permissions, ISettingRepository settings)
        {
            _db = context;
            _cache = cache;
            _permissions = permissions;
            _settings = settings;
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
            _permissions.UpdatePermissions(moduleDefinition.SiteId, EntityNames.ModuleDefinition, moduleDefinition.ModuleDefinitionId, moduleDefinition.PermissionList);
            _cache.Remove("moduledefinitions");
        }

        public void DeleteModuleDefinition(int moduleDefinitionId)
        {
            ModuleDefinition moduleDefinition = _db.ModuleDefinition.Find(moduleDefinitionId);
            _settings.DeleteSettings(EntityNames.ModuleDefinition, moduleDefinitionId);
            _db.ModuleDefinition.Remove(moduleDefinition);
            _db.SaveChanges();
            _cache.Remove("moduledefinitions");
        }

        public ModuleDefinition FilterModuleDefinition(ModuleDefinition moduleDefinition)
        {
            var ModuleDefinition = new ModuleDefinition();

            if (moduleDefinition != null)
            {
                // only include required client-side properties
                ModuleDefinition.ModuleDefinitionId = moduleDefinition.ModuleDefinitionId;
                ModuleDefinition.SiteId = moduleDefinition.SiteId;
                ModuleDefinition.ModuleDefinitionName = moduleDefinition.ModuleDefinitionName;
                ModuleDefinition.Name = moduleDefinition.Name;
                ModuleDefinition.Runtimes = moduleDefinition.Runtimes;
                ModuleDefinition.PermissionNames = moduleDefinition.PermissionNames;
                ModuleDefinition.ControlTypeRoutes = moduleDefinition.ControlTypeRoutes;
                ModuleDefinition.DefaultAction = moduleDefinition.DefaultAction;
                ModuleDefinition.SettingsType = moduleDefinition.SettingsType;
                ModuleDefinition.ControlTypeTemplate = moduleDefinition.ControlTypeTemplate;
                ModuleDefinition.IsPortable = moduleDefinition.IsPortable;
            }

            return ModuleDefinition;
        }

        public List<ModuleDefinition> LoadModuleDefinitions(int siteId)
        {
            // get module definitions
            List<ModuleDefinition> moduleDefinitions;
            if (siteId != -1)
            {
                moduleDefinitions = _cache.GetOrCreate("moduledefinitions", entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return LoadModuleDefinitions();
                });

                // get all module definition permissions for site
                List<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.ModuleDefinition).ToList();

                // populate module definition permissions
                foreach (ModuleDefinition moduledefinition in moduleDefinitions)
                {
                    moduledefinition.SiteId = siteId;
                    if (permissions.Count == 0)
                    {
                        // no module definition permissions exist for this site
                        moduledefinition.PermissionList = ClonePermissions(siteId, moduledefinition.PermissionList);
                        _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledefinition.ModuleDefinitionId, moduledefinition.PermissionList);
                    }
                    else
                    {
                        if (permissions.Any(item => item.EntityId == moduledefinition.ModuleDefinitionId))
                        {
                            moduledefinition.PermissionList = permissions.Where(item => item.EntityId == moduledefinition.ModuleDefinitionId).ToList();
                        }
                        else
                        {
                            // permissions for module definition do not exist for this site
                            moduledefinition.PermissionList = ClonePermissions(siteId, moduledefinition.PermissionList);
                            _permissions.UpdatePermissions(siteId, EntityNames.ModuleDefinition, moduledefinition.ModuleDefinitionId, moduledefinition.PermissionList);
                        }
                    }
                }

                // clean up any orphaned permissions
                var ids = new HashSet<int>(moduleDefinitions.Select(item => item.ModuleDefinitionId));
                foreach (var permission in permissions.Where(item => !ids.Contains(item.EntityId)))
                {
                    try
                    {
                        _permissions.DeletePermission(permission.PermissionId);
                    }
                    catch
                    {
                        // multi-threading can cause a race condition to occur
                    }
                }
            }
            else
            {
                moduleDefinitions = LoadModuleDefinitions();
            }

            return moduleDefinitions;
        }

        private List<ModuleDefinition> LoadModuleDefinitions()
        {
            // get module assemblies 
            List<ModuleDefinition> moduleDefinitions = LoadModuleDefinitionsFromAssemblies();

            // get module definitions in database
            List<ModuleDefinition> moduledefs = _db.ModuleDefinition.ToList();

            // sync module assemblies with database
            foreach (ModuleDefinition moduledefinition in moduleDefinitions)
            {
                ModuleDefinition moduledef = moduledefs.Where(item => item.ModuleDefinitionName == moduledefinition.ModuleDefinitionName).FirstOrDefault();
                if (moduledef == null)
                {
                    // new module definition
                    moduledef = new ModuleDefinition { ModuleDefinitionName = moduledefinition.ModuleDefinitionName };
                    _db.ModuleDefinition.Add(moduledef);
                    _db.SaveChanges();
                    moduledefinition.Version = "";
                }
                else
                {
                    // override user customizable property values
                    moduledefinition.Name = (!string.IsNullOrEmpty(moduledef.Name)) ? moduledef.Name : moduledefinition.Name;
                    moduledefinition.Description = (!string.IsNullOrEmpty(moduledef.Description)) ? moduledef.Description : moduledefinition.Description;
                    moduledefinition.Categories = (!string.IsNullOrEmpty(moduledef.Categories)) ? moduledef.Categories : moduledefinition.Categories;
                    // manage releaseversions in cases where it was not provided or is lower than the module version
                    if (string.IsNullOrEmpty(moduledefinition.ReleaseVersions) || Version.Parse(moduledefinition.Version).CompareTo(Version.Parse(moduledefinition.ReleaseVersions.Split(',').Last())) > 0)
                    {
                        moduledefinition.ReleaseVersions = moduledefinition.Version;
                    }
                    moduledefinition.Version = moduledef.Version;
                    // remove module definition from list as it is already synced
                    moduledefs.Remove(moduledef);
                }

                moduledefinition.ModuleDefinitionId = moduledef.ModuleDefinitionId;
                moduledefinition.CreatedBy = moduledef.CreatedBy;
                moduledefinition.CreatedOn = moduledef.CreatedOn;
                moduledefinition.ModifiedBy = moduledef.ModifiedBy;
                moduledefinition.ModifiedOn = moduledef.ModifiedOn;
            }

            // any remaining module definitions are orphans
            foreach (ModuleDefinition moduledefinition in moduledefs)
            {
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
                if (System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Utilities.GetTypeName(assembly.FullName) + ".dll")))
                {
                    moduleDefinitions = LoadModuleDefinitionsFromAssembly(moduleDefinitions, assembly);
                }
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
                    moduledefinition.ControlTypeTemplate = modulecontroltype.Namespace + "." + Constants.ActionToken + ", " + modulecontroltype.Assembly.GetName().Name;
                    moduledefinition.AssemblyName = assembly.GetName().Name;

                    moduledefinition.IsPortable = false;
                    if (!string.IsNullOrEmpty(moduledefinition.ServerManagerType))
                    {
                        Type servertype = Type.GetType(moduledefinition.ServerManagerType);
                        if (servertype != null && servertype.GetInterface("IPortable") != null)
                        {
                            moduledefinition.IsPortable = true;
                        }
                    }

                    if (string.IsNullOrEmpty(moduledefinition.Categories))
                    {
                        moduledefinition.Categories = "Common";
                    }

                    if (moduledefinition.Categories == "Admin")
                    {
                        moduledefinition.PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.Utilize, RoleNames.Admin, true)
                        };
                    }
                    else
                    {
                        moduledefinition.PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.Utilize, RoleNames.Admin, true),
                            new Permission(PermissionNames.Utilize, RoleNames.Registered, true)
                        };
                    }

                    Debug.WriteLine($"Oqtane Info: Registering Module {moduledefinition.ModuleDefinitionName}");
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

        private List<Permission> ClonePermissions(int siteId, List<Permission> permissionList)
        {
            var permissions = new List<Permission>();
            foreach (var p in permissionList)
            {
                var permission = new Permission();
                permission.SiteId = siteId;
                permission.EntityName = p.EntityName;
                permission.EntityId = p.EntityId;
                permission.PermissionName = p.PermissionName;
                permission.RoleId = null;
                permission.RoleName = p.RoleName;
                permission.UserId = p.UserId;
                permission.IsAuthorized = p.IsAuthorized;
                permissions.Add(permission);
            }
            return permissions;
        }
    }
}
