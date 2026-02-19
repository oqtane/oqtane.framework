using System;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteTaskBase : ISiteTask
    {
        public virtual string ExecuteTask(IServiceProvider provider, Site site, string parameters)
        {
            return "";
        }

        public virtual Task<string> ExecuteTaskAsync(IServiceProvider provider, Site site, string parameters)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
