using System.Collections.Generic;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

namespace Oqtane.Repository.Databases.Interfaces
{
    public interface IMultiDatabase
    {
        public IDatabase ActiveDatabase { get; }
    }
}
