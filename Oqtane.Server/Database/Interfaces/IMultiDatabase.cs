using Oqtane.Interfaces;

namespace Oqtane.Database
{
    public interface IMultiDatabase
    {
        public IOqtaneDatabase ActiveDatabase { get; }
    }
}
