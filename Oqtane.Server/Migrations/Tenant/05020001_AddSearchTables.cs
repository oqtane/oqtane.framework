using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.02.00.01")]
    public class AddSearchTables : MultiDatabaseMigration
    {
        public AddSearchTables(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var searchDocumentEntityBuilder = new SearchDocumentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentEntityBuilder.Create();

            var searchDocumentPropertyEntityBuilder = new SearchDocumentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentPropertyEntityBuilder.Create();

            var searchDocumentTagEntityBuilder = new SearchDocumentTagEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentTagEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var searchDocumentPropertyEntityBuilder = new SearchDocumentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentPropertyEntityBuilder.Drop();

            var searchDocumentTagEntityBuilder = new SearchDocumentTagEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentTagEntityBuilder.Drop();

            var searchDocumentEntityBuilder = new SearchDocumentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchDocumentEntityBuilder.Drop();
        }
    }
}
