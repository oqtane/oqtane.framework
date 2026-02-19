using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class JobTaskEntityBuilder : AuditableBaseEntityBuilder<JobTaskEntityBuilder>
    {
        private const string _entityTableName = "JobTask";
        private readonly PrimaryKey<JobTaskEntityBuilder> _primaryKey = new("PK_JobTask", x => x.JobTaskId);
        private readonly ForeignKey<JobTaskEntityBuilder> _siteForeignKey = new("FK_JobTask_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public JobTaskEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override JobTaskEntityBuilder BuildTable(ColumnsBuilder table)
        {
            JobTaskId = AddAutoIncrementColumn(table,"JobTaskId");
            SiteId = AddIntegerColumn(table,"SiteId");
            Name = AddStringColumn(table, "Name", 200);
            Type = AddStringColumn(table, "Type", 200);
            Parameters = AddMaxStringColumn(table, "Parameters", true);
            IsCompleted = AddBooleanColumn(table, "IsCompleted", true);
            Status = AddMaxStringColumn(table, "Status", true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> JobTaskId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Type { get; private set; }

        public OperationBuilder<AddColumnOperation> Parameters { get; private set; }

        public OperationBuilder<AddColumnOperation> IsCompleted { get; private set; }

        public OperationBuilder<AddColumnOperation> Status { get; private set; }
    }
}
