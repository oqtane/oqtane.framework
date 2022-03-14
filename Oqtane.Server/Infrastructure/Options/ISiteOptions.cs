
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISiteOptions<TOptions, TAlias>
        where TOptions : class, new()
        where TAlias : class, IAlias, new()
    {
        void Configure(TOptions options, TAlias siteOptions);
    }
}
