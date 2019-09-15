using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IAliasRepository
    {
        IEnumerable<Alias> GetAliases();
        Alias AddAlias(Alias Alias);
        Alias UpdateAlias(Alias Alias);
        Alias GetAlias(int AliasId);
        void DeleteAlias(int AliasId);
    }
}
