using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class SampleJob : HostedServiceBase
    {
        // JobType = "Oqtane.Infrastructure.SampleJob, Oqtane.Server"

        public SampleJob(IServiceScopeFactory ServiceScopeFactory) : base(ServiceScopeFactory) {}

        public override string ExecuteJob(IServiceProvider provider)
        {
            // get the first alias for this installation
            var Aliases = provider.GetRequiredService<IAliasRepository>();
            Alias alias = Aliases.GetAliases().FirstOrDefault();

            // use the SiteState to set the Alias explicitly so the tenant can be resolved
            var sitestate = provider.GetRequiredService<SiteState>();
            sitestate.Alias = alias; 

            // call a repository service which requires tenant resolution
            var Sites = provider.GetRequiredService<ISiteRepository>();
            Site site = Sites.GetSites().FirstOrDefault();  

            return "You Should Include Any Notes Related To The Execution Of The Schedule Job. This Job Simply Reports That The Default Site Is " + site.Name;
        }
    }
}
