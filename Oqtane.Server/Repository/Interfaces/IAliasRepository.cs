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
        Alias GetAlias(int aliasId, bool tracking);
        Alias GetAlias(string url);
        void DeleteAlias(int aliasId);
    }
}
