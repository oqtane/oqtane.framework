using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IAliasService
    {
        Task<List<Alias>> GetAliasesAsync();

        Task<Alias> GetAliasAsync(int aliasId);

        Task<Alias> GetAliasAsync(string url, DateTime lastSyncDate);

        Task<Alias> AddAliasAsync(Alias alias);

        Task<Alias> UpdateAliasAsync(Alias alias);

        Task DeleteAliasAsync(int aliasId);
    }
}
