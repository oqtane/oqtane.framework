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
        private readonly List<ModuleDefinition> ModuleDefinitions;

        public ModuleDefinitionRepository()
        {
            ModuleDefinitions = LoadModuleDefinitions();
        }

        private List<ModuleDefinition> LoadModuleDefinitions()
        {
            List<ModuleDefinition> ModuleDefinitions = new List<ModuleDefinition>();

            // iterate through Oqtane module assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                ModuleDefinitions = LoadModuleDefinitionsFromAssembly(ModuleDefinitions, assembly);
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
                                Version = GetProperty(properties, "Version"),
                                Owner = GetProperty(properties, "Owner"),
                                Url = GetProperty(properties, "Url"),
                                Contact = GetProperty(properties, "Contact"),
                                License = GetProperty(properties, "License"),
                                Dependencies = GetProperty(properties, "Dependencies"),
                                Permissions = GetProperty(properties, "Permissions"),
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
                                Version = new Version(1, 0, 0).ToString(),
                                Owner = "",
                                Url = "",
                                Contact = "",
                                License = "",
                                Dependencies = "",
                                Permissions = "",
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

        public IEnumerable<ModuleDefinition> GetModuleDefinitions()
        {
            return ModuleDefinitions;
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
    }
}
