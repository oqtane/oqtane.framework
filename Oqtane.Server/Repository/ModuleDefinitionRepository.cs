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
        private readonly List<ModuleDefinition> moduledefinitions;

        public ModuleDefinitionRepository()
        {
            moduledefinitions = LoadModuleDefinitions();
        }

        private List<ModuleDefinition> LoadModuleDefinitions()
        {
            List<ModuleDefinition> moduledefinitions = new List<ModuleDefinition>();

            // iterate through Oqtane module assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Oqtane.Client") || assembly.FullName.StartsWith("Oqtane.Module."))
                {
                    moduledefinitions = LoadModuleDefinitionsFromAssembly(moduledefinitions, assembly);
                }
            }

            return moduledefinitions;
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
                            .Where(item => item.Namespace.StartsWith(ModuleType))
                            .Where(item => item.GetInterfaces().Contains(typeof(IModule)))
                            .FirstOrDefault();
                        if (moduletype != null)
                        {
                            var moduleobject = Activator.CreateInstance(moduletype);
                            moduledefinition = new ModuleDefinition
                            {
                                ModuleDefinitionName = QualifiedModuleType,
                                Name = (string)moduletype.GetProperty("Name").GetValue(moduleobject),
                                Description = (string)moduletype.GetProperty("Description").GetValue(moduleobject),
                                Version = (string)moduletype.GetProperty("Version").GetValue(moduleobject),
                                Owner = (string)moduletype.GetProperty("Owner").GetValue(moduleobject),
                                Url = (string)moduletype.GetProperty("Url").GetValue(moduleobject),
                                Contact = (string)moduletype.GetProperty("Contact").GetValue(moduleobject),
                                License = (string)moduletype.GetProperty("License").GetValue(moduleobject),
                                Dependencies = (string)moduletype.GetProperty("Dependencies").GetValue(moduleobject),
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
            return moduledefinitions;
        }


    }
}
