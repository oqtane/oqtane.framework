using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Diagnostics;
using Oqtane.Modules;
using Oqtane.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Oqtane.Maui;

public static class MauiProgram
{
    // can be overridden in an appsettings.json in AppDataDirectory
    static string url = (DeviceInfo.Platform == DevicePlatform.Android)
        ? "http://10.0.2.2:44357"
        : "http://localhost:44357";

    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
        #endif

        LoadAppSettings();

        var httpClient = new HttpClient { BaseAddress = new Uri(url) };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Shared.Constants.MauiUserAgent);
        builder.Services.AddSingleton(httpClient);
        builder.Services.AddHttpClient(); // IHttpClientFactory for calling remote services via RemoteServiceBase

        // dynamically load client assemblies
        LoadClientAssemblies(httpClient);

        // register localization services
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // register auth services
        builder.Services.AddOqtaneAuthorization();

        // register scoped core services
        builder.Services.AddOqtaneScopedServices();

        var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
        foreach (var assembly in assemblies)
        {
            // dynamically register module services
            RegisterModuleServices(assembly, builder.Services);

            // register client startup services
            RegisterClientStartups(assembly, builder.Services);
        }

        return builder.Build();
	}

    private static void LoadAppSettings()
    {
        string file = Path.Combine(FileSystem.Current.AppDataDirectory, "appsettings.json");
        if (File.Exists(file))
        {
            using FileStream stream = File.OpenRead(file);
            using StreamReader reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            var obj = JsonSerializer.Deserialize<JsonObject>(content)!;
            if (!string.IsNullOrEmpty((string)obj["Url"]))
            {
                url = (string)obj["Url"];
            }
        }
    }

    private static void LoadClientAssemblies(HttpClient http)
    {
        try
        {
            // get list of loaded assemblies on the client
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToList();

            // get assemblies from server and load into client app domain
            var zip = http.GetByteArrayAsync("/api/Installation/load").Result;

            // asemblies and debug symbols are packaged in a zip file
            using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
            {
                var dlls = new Dictionary<string, byte[]>();
                var pdbs = new Dictionary<string, byte[]>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!assemblies.Contains(Path.GetFileNameWithoutExtension(entry.FullName)))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();
                            switch (Path.GetExtension(entry.FullName))
                            {
                                case ".dll":
                                    dlls.Add(entry.FullName, file);
                                    break;
                                case ".pdb":
                                    pdbs.Add(entry.FullName, file);
                                    break;
                            }
                        }
                    }
                }

                foreach (var item in dlls)
                {
                    if (pdbs.ContainsKey(item.Key))
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value), new MemoryStream(pdbs[item.Key]));
                    }
                    else
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Oqtane Error: Loading Client Assemblies {ex}");
        }
    }

    private static void RegisterModuleServices(Assembly assembly, IServiceCollection services)
    {
        // dynamically register module scoped services
        var implementationTypes = assembly.GetInterfaces<IService>();
        foreach (var implementationType in implementationTypes)
        {
            if (implementationType.AssemblyQualifiedName != null)
            {
                var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                services.AddScoped(serviceType ?? implementationType, implementationType);
            }
        }
    }

    private static void RegisterClientStartups(Assembly assembly, IServiceCollection services)
    {
        var startUps = assembly.GetInstances<IClientStartup>();
        foreach (var startup in startUps)
        {
            startup.ConfigureServices(services);
        }
    }
}
