using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Oqtane.Application.Server
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IWebHostEnvironment environment)
        {
            AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(environment.ContentRootPath, "Data"));

            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // defer server startup to Oqtane - do not modify
            services.AddOqtane(_configuration, _environment);
        }

        public void Configure(IApplicationBuilder app, IConfigurationRoot configuration, IWebHostEnvironment environment, ICorsService corsService, ICorsPolicyProvider corsPolicyProvider, ISyncManager sync)
        {
            // defer server startup to Oqtane - do not modify
            app.UseOqtane(configuration, environment, corsService, corsPolicyProvider, sync);
        }
    }
}
