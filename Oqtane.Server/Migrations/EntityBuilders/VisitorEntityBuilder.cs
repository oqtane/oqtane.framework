using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class VisitorEntityBuilder : BaseEntityBuilder<VisitorEntityBuilder>
    {
        private const string _entityTableName = "Visitor";
        private readonly PrimaryKey<VisitorEntityBuilder> _primaryKey = new("PK_Visitor", x => x.VisitorId);
        private readonly ForeignKey<VisitorEntityBuilder> _visitorForeignKey = new("FK_Visitor_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public VisitorEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_visitorForeignKey);
        }

        protected override VisitorEntityBuilder BuildTable(ColumnsBuilder table)
        {
            VisitorId = AddAutoIncrementColumn(table, "VisitorId");
            SiteId = AddIntegerColumn(table, "SiteId");
            UserId = AddIntegerColumn(table, "UserId", true);
            Visits = AddIntegerColumn(table, "Visits");
            IPAddress = AddStringColumn(table,"IPAddress", 50);
            UserAgent = AddStringColumn(table, "UserAgent", 256);
            Language = AddStringColumn(table, "Language", 50);
            CreatedOn = AddDateTimeColumn(table, "CreatedOn");
            VisitedOn = AddDateTimeColumn(table, "VisitedOn");

            return this;
        }

        public OperationBuilder<AddColumnOperation> VisitorId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> UserId { get; private set; }

        public OperationBuilder<AddColumnOperation> Visits { get; private set; }

        public OperationBuilder<AddColumnOperation> IPAddress { get; private set; }

        public OperationBuilder<AddColumnOperation> UserAgent { get; private set; }

        public OperationBuilder<AddColumnOperation> Language { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> VisitedOn { get; private set; }
    }
}
