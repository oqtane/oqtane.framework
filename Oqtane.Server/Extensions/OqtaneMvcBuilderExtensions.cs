using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneMvcBuilderExtensions
    {
        public static IMvcBuilder AddOqtaneApplicationParts(this IMvcBuilder mvcBuilder)
        {
            if (mvcBuilder is null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            // load MVC application parts from module assemblies
            foreach (var assembly in OqtaneServiceCollectionExtensions.GetOqtaneModuleAssemblies())
            {
                // check if assembly contains MVC Controllers
                if (assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Controller))).ToArray().Length > 0)
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
