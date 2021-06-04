using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class LogEntityBuilder : BaseEntityBuilder<LogEntityBuilder>
    {
        private const string _entityTableName = "Log";
        private readonly PrimaryKey<LogEntityBuilder> _primaryKey = new("PK_Log", x => x.LogId);
        private readonly ForeignKey<LogEntityBuilder> _siteForeignKey = new("FK_Log_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public LogEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override LogEntityBuilder BuildTable(ColumnsBuilder table)
        {
            LogId = AddAutoIncrementColumn(table,"LogId");
            SiteId = AddIntegerColumn(table,"SiteId", true);
            LogDate = AddDateTimeColumn(table,"LogDate");
            PageId = AddIntegerColumn(table,"PageId", true);
            ModuleId = AddIntegerColumn(table,"ModuleId", true);
            UserId = AddIntegerColumn(table,"UserId", true);
            Url = AddStringColumn(table,"Url", 2048);
            Server = AddStringColumn(table,"Server", 200);
            Category = AddStringColumn(table,"Category", 200);
            Feature = AddStringColumn(table,"Feature", 200);
            Function = AddStringColumn(table,"Function", 20);
            Level = AddStringColumn(table,"Level", 20);
            Message = AddMaxStringColumn(table,"Message");
            MessageTemplate = AddMaxStringColumn(table,"MessageTemplate");
            Exception = AddMaxStringColumn(table,"Exception", true);
            Properties = AddMaxStringColumn(table,"Properties", true);

            return this;
        }

        public OperationBuilder<AddColumnOperation> LogId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> LogDate { get; set; }

        public OperationBuilder<AddColumnOperation> PageId { get; set; }

        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> Url { get; set; }

        public OperationBuilder<AddColumnOperation> Server { get; set; }

        public OperationBuilder<AddColumnOperation> Category { get; set; }

        public OperationBuilder<AddColumnOperation> Feature { get; set; }

        public OperationBuilder<AddColumnOperation> Function { get; set; }

        public OperationBuilder<AddColumnOperation> Level { get; set; }

        public OperationBuilder<AddColumnOperation> Message { get; set; }

        public OperationBuilder<AddColumnOperation> MessageTemplate { get; set; }

        public OperationBuilder<AddColumnOperation> Exception { get; set; }

        public OperationBuilder<AddColumnOperation> Properties { get; set; }
    }
}
