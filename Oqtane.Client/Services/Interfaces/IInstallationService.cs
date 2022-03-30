using Oqtane.Models;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Services
{

    /// <summary>
    /// Service to manage (install master database / upgrade version / etc.) the installation
    /// </summary>
    public interface IInstallationService
    {
        /// <summary>
        /// Returns a status/message object with the current installation state 
        /// </summary>
        /// <returns></returns>
        Task<Installation> IsInstalled();

        /// <summary>
        /// Starts the installation process 
        /// </summary>
        /// <param name="config">connectionString, database type, alias etc.</param>
        /// <returns>internal status/message object</returns>
        Task<Installation> Install(InstallConfig config);

        /// <summary>
        /// Starts the upgrade process
        /// </summary>
        /// <returns>internal status/message object</returns>
        Task<Installation> Upgrade();

        /// <summary>
        /// Restarts the installation
        /// </summary>
        /// <returns>internal status/message object</returns>
        Task RestartAsync();

        /// <summary>
        /// Registers a new <see cref="User"/>
        /// </summary>
        /// <param name="email">Email of the user to be registered</param>
        /// <returns></returns>
        Task RegisterAsync(string email);

    }
}
