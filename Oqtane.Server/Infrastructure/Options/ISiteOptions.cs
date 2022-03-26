using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISiteOptions<TOptions>
        where TOptions : class, new()
    {
        void Configure(TOptions options, Alias alias);
    }
}
