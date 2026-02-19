using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteTaskEntityBuilder : AuditableBaseEntityBuilder<SiteTaskEntityBuilder>
    {
        private const string _entityTableName = "SiteTask";
        private readonly PrimaryKey<SiteTaskEntityBuilder> _primaryKey = new("PK_SiteTask", x => x.SiteTaskId);
        private readonly ForeignKey<SiteTaskEntityBuilder> _siteForeignKey = new("FK_SiteTask_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public SiteTaskEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override SiteTaskEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteTaskId = AddAutoIncrementColumn(table,"SiteTaskId");
            SiteId = AddIntegerColumn(table,"SiteId");
            Name = AddStringColumn(table, "Name", 200);
            Type = AddStringColumn(table, "Type", 200);
            Parameters = AddMaxStringColumn(table, "Parameters", true);
            IsCompleted = AddBooleanColumn(table, "IsCompleted", true);
            Status = AddMaxStringColumn(table, "Status", true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteTaskId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Type { get; private set; }

        public OperationBuilder<AddColumnOperation> Parameters { get; private set; }

        public OperationBuilder<AddColumnOperation> IsCompleted { get; private set; }

        public OperationBuilder<AddColumnOperation> Status { get; private set; }
    }
}
