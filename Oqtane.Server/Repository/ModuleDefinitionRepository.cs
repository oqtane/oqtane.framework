using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Reflection;
using System;
using Oqtane.Modules;

namespace Oqtane.Repository
{
    public class ModuleDefinitionRepository : IModuleDefinitionRepository
    {
        private MasterDBContext db;
        private readonly IPermissionRepository Permissions;

        public ModuleDefinitionRepository(MasterDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
        }

        private List<ModuleDefinition> LoadModuleDefinitions(int SiteId)
        {
            List<ModuleDefinition> ModuleDefinitions = new List<ModuleDefinition>();

            // iterate through Oqtane module assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                ModuleDefinitions = LoadModuleDefinitionsFromAssembly(ModuleDefinitions, assembly);
            }

            // sync module definitions with database
            List<ModuleDefinition> moduledefs = db.ModuleDefinition.ToList();
            foreach (ModuleDefinition moduledefinition in ModuleDefinitions)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions(SiteId, "ModuleDefinition").ToList();
                ModuleDefinition moduledef = moduledefs.Where(item => item.ModuleDefinitionName == moduledefinition.ModuleDefinitionName).FirstOrDefault();
                if (moduledef == null)
                {
                    moduledef = new ModuleDefinition { ModuleDefinitionName = moduledefinition.ModuleDefinitionName };
                    db.ModuleDefinition.Add(moduledef);
                    db.SaveChanges();
                }
                moduledefinition.ModuleDefinitionId = moduledef.ModuleDefinitionId;
                moduledefinition.SiteId = SiteId;
                moduledefinition.Permissions = Permissions.EncodePermissions(moduledefinition.ModuleDefinitionId, permissions);
                moduledefinition.CreatedBy = moduledef.CreatedBy;
                moduledefinition.CreatedOn = moduledef.CreatedOn;
                moduledefinition.ModifiedBy = moduledef.ModifiedBy;
                moduledefinition.ModifiedOn = moduledef.ModifiedOn;
            }

            return ModuleDefinitions;
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
                    string ModuleType = string.Join(".", segments);
                    string QualifiedModuleType = ModuleType + ", " + typename[1];

                    int index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == QualifiedModuleType);
                    if (index == -1)
                    {
                        /// determine if this module implements IModule
                        Type moduletype = assembly.GetTypes()
                        .Where(item => item.Namespace != null)
                        .Where(item => item.Namespace.StartsWith(ModuleType))
                        .Where(item => item.GetInterfaces().Contains(typeof(IModule)))
                        .FirstOrDefault();
                        if (moduletype != null)
                        {
                            var moduleobject = Activator.CreateInstance(moduletype);
                            Dictionary<string, string> properties = (Dictionary<string, string>)moduletype.GetProperty("Properties").GetValue(moduleobject);
                            moduledefinition = new ModuleDefinition
                            {
                                ModuleDefinitionName = QualifiedModuleType,
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
                                ControlTypeTemplate = ModuleType + ".{Control}" + ", " + typename[1],
                                ControlTypeRoutes = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        else
                        {
                            moduledefinition = new ModuleDefinition
                            {
                                ModuleDefinitionName = QualifiedModuleType,
                                Name = ModuleType.Substring(ModuleType.LastIndexOf(".") + 1),
                                Description = ModuleType.Substring(ModuleType.LastIndexOf(".") + 1),
                                Categories = ((QualifiedModuleType.StartsWith("Oqtane.Modules.Admin.")) ? "Admin" : ""),
                                Version = new Version(1, 0, 0).ToString(),
                                Owner = "",
                                Url = "",
                                Contact = "",
                                License = "",
                                Dependencies = "",
                                PermissionNames = "",
                                ControlTypeTemplate = ModuleType + ".{Control}" + ", " + typename[1],
                                ControlTypeRoutes = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        moduledefinitions.Add(moduledefinition);
                        index = moduledefinitions.FindIndex(item => item.ModuleDefinitionName == QualifiedModuleType);
                    }
                    moduledefinition = moduledefinitions[index];
                    // actions
                    var modulecontrolobject = Activator.CreateInstance(modulecontroltype);
                    string actions = (string)modulecontroltype.GetProperty("Actions").GetValue(modulecontrolobject);
                    if (actions != "")
                    {
                        foreach(string action in actions.Split(','))
                        {
                            moduledefinition.ControlTypeRoutes += (action + "=" + modulecontroltype.FullName + ", " + typename[1] + ";");
                        }
                    }
                    moduledefinitions[index] = moduledefinition;
                }
            }

            return moduledefinitions;
        }

        private string GetProperty(Dictionary<string, string> Properties, string Key)
        {
            string Value = "";
            if (Properties.ContainsKey(Key))
            {
                Value = Properties[Key];
            }
            return Value;
        }

        public IEnumerable<ModuleDefinition> GetModuleDefinitions(int SiteId)
        {
            return LoadModuleDefinitions(SiteId);
        }

        public void UpdateModuleDefinition(ModuleDefinition ModuleDefinition)
        {
            Permissions.UpdatePermissions(ModuleDefinition.SiteId, "ModuleDefinition", ModuleDefinition.ModuleDefinitionId, ModuleDefinition.Permissions);
        }
    }
}
