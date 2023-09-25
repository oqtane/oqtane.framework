using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
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
        private readonly ITenantManager _tenants;
        private readonly ISettingRepository _settings;
        private readonly IServerStateManager _serverState;
        private readonly string settingprefix = "SiteEnabled:";

        public ModuleDefinitionRepository(MasterDBContext context, IMemoryCache cache, IPermissionRepository permissions, ITenantManager tenants, ISettingRepository settings, IServerStateManager serverState)
        {
            _db = context;
            _cache = cache;
            _permissions = permissions;
            _tenants = tenants;
            _settings = settings;
            _serverState = serverState;
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

            var settingname = $"{settingprefix}{_tenants.GetAlias().SiteKey}";
            var setting = _settings.GetSetting(EntityNames.ModuleDefinition, moduleDefinition.ModuleDefinitionId, settingname);
            if (setting == null)
            {
                _settings.AddSetting(new Setting { EntityName = EntityNames.ModuleDefinition, EntityId = moduleDefinition.ModuleDefinitionId, SettingName = settingname, SettingValue = moduleDefinition.IsEnabled.ToString(), IsPrivate = true });
            }
            else
            {
                setting.SettingValue = moduleDefinition.IsEnabled.ToString();
                _settings.UpdateSetting(setting);
            }

            _cache.Remove($"moduledefinitions:{_tenants.GetAlias().SiteKey}");
        }

        public void DeleteModuleDefinition(int moduleDefinitionId)
        {
            ModuleDefinition moduleDefinition = _db.ModuleDefinition.Find(moduleDefinitionId);
            _settings.DeleteSettings(EntityNames.ModuleDefinition, moduleDefinitionId);
            _db.ModuleDefinition.Remove(moduleDefinition);
            _db.SaveChanges();
            _cache.Remove($"moduledefinitions:{_tenants.GetAlias().SiteKey}");
        }

        public ModuleDefinition FilterModuleDefinition(ModuleDefinition moduleDefinition)
        {
            ModuleDefinition ModuleDefinition = null;

            if (moduleDefinition != null)
            {
                // only include required client-side properties
                ModuleDefinition = new ModuleDefinition();
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
                ModuleDefinition.Resources = moduleDefinition.Resources;
                ModuleDefinition.IsEnabled = moduleDefinition.IsEnabled;
                ModuleDefinition.PackageName = moduleDefinition.PackageName;
            }

            return ModuleDefinition;
        }

        public List<ModuleDefinition> LoadModuleDefinitions(int siteId)
        {
            // get module definitions
            List<ModuleDefinition> moduleDefinitions;
            if (siteId != -1)
            {
                moduleDefinitions = _cache.GetOrCreate($"moduledefinitions:{_tenants.GetAlias().SiteKey}", entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return ProcessModuleDefinitions(siteId);
                });
            }
            else // called during startup
            {
                return ProcessModuleDefinitions(-1);
            }

            return moduleDefinitions;
        }

        private List<ModuleDefinition> ProcessModuleDefinitions(int siteId)
        {
            // get module assemblies 
            List<ModuleDefinition> ModuleDefinitions = LoadModuleDefinitionsFromAssemblies();

            // get module definitions in database
            List<ModuleDefinition> moduledefinitions = _db.ModuleDefinition.ToList();

            // sync module assemblies with database
            foreach (ModuleDefinition ModuleDefinition in ModuleDefinitions)
            {
                // manage releaseversions in cases where it was not provided or is lower than the module version
                if (string.IsNullOrEmpty(ModuleDefinition.ReleaseVersions) || (!string.IsNullOrEmpty(ModuleDefinition.Version) && Version.Parse(ModuleDefinition.Version).CompareTo(Version.Parse(ModuleDefinition.ReleaseVersions.Split(',').Last())) > 0))
                {
                    ModuleDefinition.ReleaseVersions = ModuleDefinition.Version;
                }

                ModuleDefinition moduledefinition = moduledefinitions.Where(item => item.ModuleDefinitionName == ModuleDefinition.ModuleDefinitionName).FirstOrDefault();

                if (moduledefinition == null)
                {
                    // new module definition
                    moduledefinition = new ModuleDefinition { ModuleDefinitionName = ModuleDefinition.ModuleDefinitionName };
                    _db.ModuleDefinition.Add(moduledefinition);
                    _db.SaveChanges();
                    // version is explicitly not set because it is updated as part of module migrations at startup
                    ModuleDefinition.Version = ""; 
                }
                else
                {
                    // override user customizable property values
                    ModuleDefinition.Name = (!string.IsNullOrEmpty(moduledefinition.Name)) ? moduledefinition.Name : ModuleDefinition.Name;
                    ModuleDefinition.Description = (!string.IsNullOrEmpty(moduledefinition.Description)) ? moduledefinition.Description : ModuleDefinition.Description;
                    ModuleDefinition.Categories = (!string.IsNullOrEmpty(moduledefinition.Categories)) ? moduledefinition.Categories : ModuleDefinition.Categories;
                    // get current version
                    ModuleDefinition.Version = moduledefinition.Version;

                    // remove module definition from list as it is already synced
                    moduledefinitions.Remove(moduledefinition);
                }

                // load db properties
                ModuleDefinition.ModuleDefinitionId = moduledefinition.ModuleDefinitionId;
                ModuleDefinition.CreatedBy = moduledefinition.CreatedBy;
                ModuleDefinition.CreatedOn = moduledefinition.CreatedOn;
                ModuleDefinition.ModifiedBy = moduledefinition.ModifiedBy;
                ModuleDefinition.ModifiedOn = moduledefinition.ModifiedOn;
            }

            // any remaining module definitions are orphans
            foreach (ModuleDefinition moduledefinition in moduledefinitions)
            {
                _db.ModuleDefinition.Remove(moduledefinition); // delete
                _db.SaveChanges();
            }

            if (siteId != -1)
            {
                var siteKey = _tenants.GetAlias().SiteKey;

                // get all module definition permissions for site
                List<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.ModuleDefinition).ToList();

                // get settings for site
                var settings = _settings.GetSettings(EntityNames.ModuleDefinition).ToList();

                // populate module definition site settings and permissions
                var serverState = _serverState.GetServerState(siteKey);
                foreach (ModuleDefinition moduledefinition in ModuleDefinitions)
                {
                    moduledefinition.SiteId = siteId;

                    var setting = settings.FirstOrDefault(item => item.EntityId == moduledefinition.ModuleDefinitionId && item.SettingName == $"{settingprefix}{_tenants.GetAlias().SiteKey}");
                    if (setting != null)
                    {
                       moduledefinition.IsEnabled = bool.Parse(setting.SettingValue);
                    }
                    else
                    {
                        moduledefinition.IsEnabled = moduledefinition.IsAutoEnabled;
                    }

                    if (moduledefinition.IsEnabled)
                    {
                        // build list of assemblies for site
                        if (!serverState.Assemblies.Contains(moduledefinition.AssemblyName))
                        {
                            serverState.Assemblies.Add(moduledefinition.AssemblyName);
                        }
                        if (!string.IsNullOrEmpty(moduledefinition.Dependencies))
                        {
                            foreach (var assembly in moduledefinition.Dependencies.Replace(".dll", "").Split(',', StringSplitOptions.RemoveEmptyEntries).Reverse())
                            {
                                if (!serverState.Assemblies.Contains(assembly.Trim()))
                                {
                                    serverState.Assemblies.Insert(0, assembly.Trim());
                                }
                            }
                        }
                        // build list of scripts for site
                        if (moduledefinition.Resources != null)
                        {
                            foreach (var resource in moduledefinition.Resources.Where(item => item.Level == ResourceLevel.Site))
                            {
                                if (!serverState.Scripts.Contains(resource))
                                {
                                    serverState.Scripts.Add(resource);
                                }
                            }
                        }
                    }

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
                var ids = new HashSet<int>(ModuleDefinitions.Select(item => item.ModuleDefinitionId));
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

            return ModuleDefinitions;
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
                    if (moduledefinition.Resources != null)
                    {
                        foreach (var resource in moduledefinition.Resources)
                        {
                            if (resource.Url.StartsWith("~"))
                            {
                                resource.Url = resource.Url.Replace("~", "/Modules/" + Utilities.GetTypeName(moduledefinition.ModuleDefinitionName) + "/").Replace("//", "/");
                            }
                        }
                    }

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
                        var shortName = moduledefinition.ModuleDefinitionName.Replace("Oqtane.Modules.Admin.", "").Replace(", Oqtane.Client", "");
                        if (Constants.DefaultHostModuleTypes.Contains(shortName))
                        {
                            moduledefinition.PermissionList = new List<Permission>
                            {
                                new Permission(PermissionNames.Utilize, RoleNames.Host, true)
                            };
                        }
                        else
                        {
                            moduledefinition.PermissionList = new List<Permission>
                            {
                                new Permission(PermissionNames.Utilize, RoleNames.Admin, true)
                            };
                        }
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
