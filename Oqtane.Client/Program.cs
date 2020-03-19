// DO NOT REMOVE - needed for client-side Blazor
using Microsoft.AspNetCore.Blazor.Hosting;

namespace Oqtane.Client
{
    public class Program
    {
#if DEBUG || RELEASE
        public static void Main(string[] args)
        {
        }
#endif

#if WASM
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
#endif
    }
}
