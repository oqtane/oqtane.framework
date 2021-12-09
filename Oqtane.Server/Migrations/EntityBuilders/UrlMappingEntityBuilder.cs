using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class UrlMappingEntityBuilder : BaseEntityBuilder<UrlMappingEntityBuilder>
    {
        private const string _entityTableName = "UrlMapping";
        private readonly PrimaryKey<UrlMappingEntityBuilder> _primaryKey = new("PK_UrlMapping", x => x.UrlMappingId);
        private readonly ForeignKey<UrlMappingEntityBuilder> _urlMappingForeignKey = new("FK_UrlMapping_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public UrlMappingEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_urlMappingForeignKey);
        }

        protected override UrlMappingEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UrlMappingId = AddAutoIncrementColumn(table, "UrlMappingId");
            SiteId = AddIntegerColumn(table, "SiteId");
            Url = AddStringColumn(table, "Url", 500);
            MappedUrl = AddStringColumn(table, "MappedUrl", 500);
            Requests = AddIntegerColumn(table, "Requests");
            CreatedOn = AddDateTimeColumn(table, "CreatedOn");
            RequestedOn = AddDateTimeColumn(table, "RequestedOn");

            return this;
        }

        public OperationBuilder<AddColumnOperation> UrlMappingId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Url { get; private set; }

        public OperationBuilder<AddColumnOperation> MappedUrl { get; private set; }

        public OperationBuilder<AddColumnOperation> Requests { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> RequestedOn { get; private set; }
    }
}
