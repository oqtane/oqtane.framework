using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(builder.Environment.ContentRootPath, "Data"));

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            builder.Services.AddOqtane(configuration, builder.Environment);

            var app = builder.Build();

            var corsService = app.Services.GetRequiredService<ICorsService>();
            var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();
            var syncManager = app.Services.GetRequiredService<ISyncManager>();

            app.UseOqtane(configuration, builder.Environment, corsService, corsPolicyProvider, syncManager);

            var databaseManager = app.Services.GetService<IDatabaseManager>();
            var install = databaseManager.Install();
            if (!string.IsNullOrEmpty(install.Message))
            {
                var filelogger = app.Services.GetRequiredService<ILogger<Program>>();
                if (filelogger != null)
                {
                    filelogger.LogError($"[Oqtane.Server.Program.Main] {install.Message}");
                }
            }
            else
            {
                app.Run();
            }
        }
    }
}
