using System.Collections.Generic;
using Oqtane.Interfaces;

namespace Oqtane.Repository.Databases.Interfaces
{
    public interface IMultiDatabase
    {
        public IEnumerable<IOqtaneDatabase> Databases { get; }
    }
}
