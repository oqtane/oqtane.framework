using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and store <see cref="Alias"/> information.
    /// </summary>
    public interface IAliasService
    {
        /// <summary>
        /// Get all aliases in the system
        /// </summary>
        /// <returns></returns>
        Task<List<Alias>> GetAliasesAsync();

        /// <summary>
        /// Get a single alias
        /// </summary>
        /// <param name="aliasId">The <see cref="Oqtane.Models.Alias"/> ID, not to be confused with a <see cref="Oqtane.Models.Site"/> ID</param>
        /// <returns></returns>
        Task<Alias> GetAliasAsync(int aliasId);

        /// <summary>
        /// Retrieve the Alias object of a URL.
        /// </summary>
        /// <param name="url">The URL - todoc - is this only the root, or can it be a longer path?</param>
        /// <param name="lastSyncDate">todoc - unclear what this is for</param>
        /// <returns></returns>
        Task<Alias> GetAliasAsync(string url, DateTime lastSyncDate);

        /// <summary>
        /// Save another <see cref="Oqtane.Models.Alias"/> in the DB. It must already contain all the information incl. <see cref="Oqtane.Models.Tenant"/> it belongs to. 
        /// </summary>
        /// <param name="alias">An <see cref="Oqtane.Models.Alias"/> to add.</param>
        /// <returns></returns>
        Task<Alias> AddAliasAsync(Alias alias);

        /// <summary>
        /// Update an <see cref="Oqtane.Models.Alias"/> in the DB. Make sure the object is correctly filled, as it must update an existing record. 
        /// </summary>
        /// <param name="alias">The <see cref="Oqtane.Models.Alias"/> to update.</param>
        /// <returns></returns>
        Task<Alias> UpdateAliasAsync(Alias alias);

        /// <summary>
        /// Remove an <see cref="Oqtane.Models.Alias"/> from the DB. 
        /// </summary>
        /// <param name="aliasId">The Alias ID, not to be confused with a Site ID.</param>
        /// <returns></returns>
        Task DeleteAliasAsync(int aliasId);
    }
}
