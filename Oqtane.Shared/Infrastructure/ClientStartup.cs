using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;

namespace Oqtane.Infrastructure
{
    public abstract class ClientStartup : IClientStartup
    {
        public virtual int Order => 0;

        public virtual void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
