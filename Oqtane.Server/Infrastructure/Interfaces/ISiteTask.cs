using System;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISiteTask
    {
        string ExecuteTask(IServiceProvider provider, Site site, string parameters);

        Task<string> ExecuteTaskAsync(IServiceProvider provider, Site site, string parameters);
    }
}
