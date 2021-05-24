using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class NotificationEntityBuilder : DeletableBaseEntityBuilder<NotificationEntityBuilder>
    {
        private const string _entityTableName = "Notification";
        private readonly PrimaryKey<NotificationEntityBuilder> _primaryKey = new("PK_Notification", x => x.NotificationId);
        private readonly ForeignKey<NotificationEntityBuilder> _siteForeignKey = new("FK_Notification_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public NotificationEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override NotificationEntityBuilder BuildTable(ColumnsBuilder table)
        {
            NotificationId = AddAutoIncrementColumn(table,"NotificationId");
            SiteId = AddIntegerColumn(table,"SiteId");
            FromUserId = AddIntegerColumn(table,"FromUserId", true);
            ToUserId = AddIntegerColumn(table,"ToUserId", true);
            ToEmail = AddStringColumn(table,"ToEmail", 256);
            ParentId = AddIntegerColumn(table,"ParentId", true);
            Subject = AddStringColumn(table,"Subject", 256);
            Body = AddMaxStringColumn(table,"Body");
            CreatedOn = AddDateTimeColumn(table,"CreatedOn");
            IsDelivered = AddBooleanColumn(table,"IsDelivered");
            DeliveredOn = AddDateTimeColumn(table,"DeliveredOn", true);

            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> NotificationId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> FromUserId { get; set; }

        public OperationBuilder<AddColumnOperation> ToUserId { get; set; }

        public OperationBuilder<AddColumnOperation> ToEmail { get; set; }

        public OperationBuilder<AddColumnOperation> ParentId { get; set; }

        public OperationBuilder<AddColumnOperation> Subject { get; set; }

        public OperationBuilder<AddColumnOperation> Body { get; set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; set; }

        public OperationBuilder<AddColumnOperation> IsDelivered { get; set; }

        public OperationBuilder<AddColumnOperation> DeliveredOn { get; set; }
    }
}
