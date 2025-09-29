using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IMigrationHistoryRepository
    {
        IEnumerable<MigrationHistory> GetMigrationHistory();
    }
    public class MigrationHistoryRepository : IMigrationHistoryRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public MigrationHistoryRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<MigrationHistory> GetMigrationHistory()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.MigrationHistory.ToList();
        }
    }
}
