
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Services
{
    public interface IClientStartup
    {
        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <remarks>The default services should (only) have negative values, so they can be executed first.</remarks>
        int Order { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        void ConfigureServices(IServiceCollection services);
    }
}
