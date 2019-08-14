using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcModuleExtensions
    {
        public static IMvcBuilder AddModuleAssemblies(this IMvcBuilder mvcBuilder, List<Assembly> assemblies)
        {
            // load MVC application parts from module assemblies
            foreach (Assembly assembly in assemblies)
            {
                // check if assembly contains MVC Controllers
                if (assembly.GetTypes().Where(item => item.IsSubclassOf(typeof(Controller))).ToArray().Length > 0)
                {
                    var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var part in partFactory.GetApplicationParts(assembly))
                    {
                        mvcBuilder.PartManager.ApplicationParts.Add(part);
                    }
                }
            }
            return mvcBuilder;
        }
    }
}