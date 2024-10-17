using Oqtane.Databases.Interfaces;

namespace Oqtane.Repository.Databases.Interfaces
{
    public interface IMultiDatabase
    {
        public IDatabase ActiveDatabase { get; }
    }
}
