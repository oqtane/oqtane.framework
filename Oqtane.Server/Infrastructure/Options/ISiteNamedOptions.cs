using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISiteNamedOptions<TOptions>
        where TOptions : class, new()
    {
        void Configure(string name, TOptions options, Alias alias, Dictionary<string, string> sitesettings);
    }
}
