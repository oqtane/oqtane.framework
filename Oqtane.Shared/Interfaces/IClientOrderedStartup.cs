using Oqtane.Services;

namespace Oqtane.Infrastructure
{
    public interface IClientOrderedStartup : IClientStartup
    {
        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <remarks>The default services should (only) have negative values, so they can be executed first.</remarks>
        int Order { get; }
    }
}
