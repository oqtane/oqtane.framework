using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;

namespace Oqtane.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureOqtaneAssemblies(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var startUps = AppDomain.CurrentDomain
                .GetOqtaneAssemblies()
                .SelectMany(x => x.GetInstances<IServerStartup>());

            foreach (var startup in startUps)
            {
                startup.Configure(app, env);
            }
            
            return app;
        }
    }
}
