using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IAliasRepository
    {
        IEnumerable<Alias> GetAliases();
        Alias AddAlias(Alias alias);
        Alias UpdateAlias(Alias alias);
        Alias GetAlias(int aliasId);
        void DeleteAlias(int aliasId);
    }
}
