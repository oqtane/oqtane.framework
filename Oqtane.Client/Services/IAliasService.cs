using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IAliasService
    {
        Task<List<Alias>> GetAliasesAsync();

        Task<Alias> GetAliasAsync(int AliasId);

        Task AddAliasAsync(Alias alias);

        Task UpdateAliasAsync(Alias alias);

        Task DeleteAliasAsync(int AliasId);
    }
}
