using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IMigrationHistoryRepository
    {
        IEnumerable<MigrationHistory> GetMigrationHistory();
    }
    public class MigrationHistoryRepository : IMigrationHistoryRepository
    {
        private MasterDBContext _db;

        public MigrationHistoryRepository(MasterDBContext context)
        {
            _db = context;
        }

        public IEnumerable<MigrationHistory> GetMigrationHistory()
        {
            return _db.MigrationHistory.ToList();
        }
    }
}
