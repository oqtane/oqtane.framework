using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Diagnostics;
using Oqtane.Modules;
using Oqtane.Services;
using System.Globalization;
using System.Text.Json;

namespace Oqtane.Maui;

public static class MauiProgram
{
    // the API service url
    static string apiurl = "https://www.dnfprojects.com"; // for testing
    //static string apiurl = "http://localhost:44357"; // for local development (Oqtane.Server must be already running for MAUI client to connect)

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

        var httpClient = new HttpClient { BaseAddress = new Uri(apiurl) };
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

    private static void LoadClientAssemblies(HttpClient http)
    {
        try
        {
            // ensure local assembly folder exists
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, "oqtane");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var dlls = new Dictionary<string, byte[]>();
            var pdbs = new Dictionary<string, byte[]>();
            var list = new List<string>();

            var files = new List<string>();
            foreach (var file in Directory.EnumerateFiles(folder, "*.dll", SearchOption.AllDirectories))
            {
                files.Add(file.Substring(folder.Length + 1).Replace("\\", "/"));
            }

            if (files.Count() != 0)
            {
                // get list of assemblies from server
                var json = Task.Run(() => http.GetStringAsync("/api/Installation/list")).GetAwaiter().GetResult();
                var assemblies = JsonSerializer.Deserialize<List<string>>(json);

                // determine which assemblies need to be downloaded
                foreach (var assembly in assemblies)
                {
                    var file = files.FirstOrDefault(item => item.Contains(assembly));
                    if (file == null)
                    {
                        list.Add(assembly);
                    }
                    else
                    {
                        // check if newer version available
                        if (GetFileDate(assembly) > GetFileDate(file))
                        {
                            list.Add(assembly);
                        }
                    }
                }

                // get assemblies already downloaded
                foreach (var file in files)
                {
                    if (assemblies.Contains(file) && !list.Contains(file))
                    {
                        try
                        {
                            dlls.Add(file, File.ReadAllBytes(Path.Combine(folder, file)));
                            var pdb = file.Replace(".dll", ".pdb");
                            if (File.Exists(Path.Combine(folder, pdb)))
                            {
                                pdbs.Add(pdb, File.ReadAllBytes(Path.Combine(folder, pdb)));
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    else // file is deprecated
                    {
                        try
                        {
                            foreach (var path in Directory.EnumerateFiles(folder, Path.GetFileNameWithoutExtension(file) + ".*"))
                            {
                                File.Delete(path);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
            else
            {
                list.Add("*");
            }

            if (list.Count != 0)
            {
                // get assemblies from server
                var zip = Task.Run(() => http.GetByteArrayAsync("/api/Installation/load?list=" + string.Join(",", list))).GetAwaiter().GetResult();

                // asemblies and debug symbols are packaged in a zip file
                using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();

                            // save assembly to local folder
                            try
                            {
                                using var stream = File.Create(Path.Combine(folder, entry.FullName));
                                stream.Write(file, 0, file.Length);
                            }
                            catch
                            {
                                // ignore
                            }

                            if (Path.GetExtension(entry.FullName) == ".dll")
                            {
                                dlls.Add(entry.FullName, file);
                            }
                            else
                            {
                                pdbs.Add(entry.FullName, file);
                            }
                        }
                    }
                }
            }

            // load assemblies into app domain
            foreach (var item in dlls)
            {
                if (pdbs.ContainsKey(item.Key.Replace(".dll", ".pdb")))
                {
                    AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value), new MemoryStream(pdbs[item.Key.Replace(".dll", ".pdb")]));
                }
                else
                {
                    AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Oqtane Error: Loading Client Assemblies {ex}");
        }
    }

    private static DateTime GetFileDate(string filepath)
    {
        var segments = filepath.Split('.');
        return DateTime.ParseExact(segments[segments.Length - 2], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }

    private static void RegisterModuleServices(Assembly assembly, IServiceCollection services)
    {
        // dynamically register module scoped services
        try
        {
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
        catch
        {
            // could not interrogate assembly - likely missing dependencies
        }
    }

    private static void RegisterClientStartups(Assembly assembly, IServiceCollection services)
    {
        try
        {
            var startUps = assembly.GetInstances<IClientStartup>();
            foreach (var startup in startUps)
            {
                startup.ConfigureServices(services);
            }
        }
        catch
        {
            // could not interrogate assembly - likely missing dependencies
        }
    }
}
