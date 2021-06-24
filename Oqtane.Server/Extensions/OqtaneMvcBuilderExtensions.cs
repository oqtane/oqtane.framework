using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Oqtane.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // check if assembly contains MVC Controllers
                if (assembly.GetTypes().Any(t => t.IsSubclassOf(typeof(Controller)) || t.IsSubclassOf(typeof(Page))))
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


        public static IMvcBuilder ConfigureOqtaneMvc(this IMvcBuilder mvcBuilder)
        {
            var startUps = AppDomain.CurrentDomain
                .GetOqtaneAssemblies()
                .SelectMany(x => x.GetInstances<IServerStartup>());

            foreach (var startup in startUps)
            {
                startup.ConfigureMvc(mvcBuilder);
            }

            return mvcBuilder;
        }
    }
}
