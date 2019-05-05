using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using System.Reflection;
using System;
using Oqtane.Skins;

namespace Oqtane.Repository
{
    public class SkinRepository : ISkinRepository
    {
        private readonly List<Skin> skins;

        public SkinRepository()
        {
            skins = LoadSkins();
        }

        private List<Skin> LoadSkins()
        {
            List<Skin> skins = new List<Skin>();

            // iterate through Oqtane skin assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Oqtane.Client") || assembly.FullName.StartsWith("Oqtane.Skin."))
                {
                    skins = LoadSkinsFromAssembly(skins, assembly);
                }
            }

            return skins;
        }

        private List<Skin> LoadSkinsFromAssembly(List<Skin> skins, Assembly assembly)
        {
            Skin skin;
            Type[] skincontroltypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ISkinControl))).ToArray();
            foreach (Type skincontroltype in skincontroltypes)
            {
                if (skincontroltype.Name != "SkinBase")
                {
                    string[] typename = skincontroltype.AssemblyQualifiedName.Split(',').Select(item => item.Trim()).ToList().ToArray();
                    string[] segments = typename[0].Split('.');
                    Array.Resize(ref segments, segments.Length - 1);
                    string Namespace = string.Join(".", segments);

                    int index = skins.FindIndex(item => item.SkinName == Namespace);
                    if (index == -1)
                    {
                        /// determine if this skin implements ISkin
                        Type skintype = assembly.GetTypes()
                            .Where(item => item.Namespace.StartsWith(Namespace))
                            .Where(item => item.GetInterfaces().Contains(typeof(ISkin))).FirstOrDefault();
                        if (skintype != null)
                        {
                            var skinobject = Activator.CreateInstance(skintype);
                            skin = new Skin
                            {
                                SkinName = Namespace,
                                Name = (string)skintype.GetProperty("Name").GetValue(skinobject),
                                Version = (string)skintype.GetProperty("Version").GetValue(skinobject),
                                Owner = (string)skintype.GetProperty("Owner").GetValue(skinobject),
                                Url = (string)skintype.GetProperty("Url").GetValue(skinobject),
                                Contact = (string)skintype.GetProperty("Contact").GetValue(skinobject),
                                License = (string)skintype.GetProperty("License").GetValue(skinobject),
                                Dependencies = (string)skintype.GetProperty("Dependencies").GetValue(skinobject),
                                SkinControls = "",
                                PaneLayouts = "",
                                ContainerControls = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        else
                        {
                            skin = new Skin
                            {
                                SkinName = Namespace,
                                Name = skincontroltype.Name,
                                Version = new Version(1, 0, 0).ToString(),
                                Owner = "",
                                Url = "",
                                Contact = "",
                                License = "",
                                Dependencies = "",
                                SkinControls = "",
                                PaneLayouts = "",
                                ContainerControls = "",
                                AssemblyName = assembly.FullName.Split(",")[0]
                            };
                        }
                        skins.Add(skin);
                        index = skins.FindIndex(item => item.SkinName == Namespace);
                    }
                    skin = skins[index];
                    // layouts and skins
                    if (skincontroltype.FullName.EndsWith("Layout"))
                    {
                        skin.PaneLayouts += (skincontroltype.FullName + ", " + typename[1] + ";");
                    }
                    else
                    {
                        skin.SkinControls += (skincontroltype.FullName + ", " + typename[1] + ";");
                    }
                    // containers
                    Type[] containertypes = assembly.GetTypes().Where(item => item.Namespace.StartsWith(Namespace)).Where(item => item.GetInterfaces().Contains(typeof(IContainerControl))).ToArray();
                    foreach (Type containertype in containertypes)
                    {
                        skin.ContainerControls += (containertype.FullName + ", " + typename[1] + ";");
                    }
                    skins[index] = skin;
                }
            }
            return skins;
        }

        public IEnumerable<Skin> GetSkins()
        {
            return skins;
        }
    }
}
