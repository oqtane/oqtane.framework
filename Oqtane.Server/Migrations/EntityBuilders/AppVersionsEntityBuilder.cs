using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class AppVersionsEntityBuilder : BaseEntityBuilder<AppVersionsEntityBuilder>
    {
        private const string _entityTableName = "AppVersions";
        private readonly PrimaryKey<AppVersionsEntityBuilder> _primaryKey = new("PK_AppVersions", x => x.Id);

        public AppVersionsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override AppVersionsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table,"Id");
            Version = AddStringColumn(table,"Version", 10);
            AppliedDate = AddDateTimeColumn(table,"AppliedDate");

            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }

        public OperationBuilder<AddColumnOperation> Version { get; set; }

        public OperationBuilder<AddColumnOperation> AppliedDate { get; set; }

    }
}
