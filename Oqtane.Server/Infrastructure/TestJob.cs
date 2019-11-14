using System;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class TestJob : HostedServiceBase
    {
        public TestJob(IServiceScopeFactory ServiceScopeFactory) : base(ServiceScopeFactory) {}

        public override void ExecuteJob(IServiceProvider provider)
        {
            var Tenants = provider.GetRequiredService<ITenantRepository>();
            foreach(Tenant tenant in Tenants.GetTenants())
            {
                // is it possible to set the HttpContext so that DbContextBase will resolve properly for tenants?
            }
        }
    }
}
