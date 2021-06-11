using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ProfileEntityBuilder : AuditableBaseEntityBuilder<ProfileEntityBuilder>
    {
        private const string _entityTableName = "Profile";
        private readonly PrimaryKey<ProfileEntityBuilder> _primaryKey = new("PK_Profile", x => x.ProfileId);
        private readonly ForeignKey<ProfileEntityBuilder> _siteForeignKey = new("FK_Profile_Sites", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public ProfileEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override ProfileEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ProfileId = AddAutoIncrementColumn(table,"ProfileId");
            SiteId = AddIntegerColumn(table,"SiteId", true);
            Name = AddStringColumn(table,"Name", 50);
            Title = AddStringColumn(table,"Title", 50);
            Description = AddStringColumn(table,"Description", 256, true);
            Category = AddStringColumn(table,"Category", 50);
            ViewOrder = AddIntegerColumn(table,"ViewOrder");
            MaxLength = AddIntegerColumn(table,"MaxLength");
            DefaultValue = AddStringColumn(table,"DefaultValue", 2000, true);
            IsRequired = AddBooleanColumn(table,"IsRequired");
            IsPrivate = AddBooleanColumn(table,"IsPrivate");

            AddAuditableColumns(table);

            return this;    }

        public OperationBuilder<AddColumnOperation> ProfileId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Title { get; set; }

        public OperationBuilder<AddColumnOperation> Description { get; set; }

        public OperationBuilder<AddColumnOperation> Category { get; set; }

        public OperationBuilder<AddColumnOperation> ViewOrder { get; set; }

        public OperationBuilder<AddColumnOperation> MaxLength { get; set; }

        public OperationBuilder<AddColumnOperation> DefaultValue { get; set; }

        public OperationBuilder<AddColumnOperation> IsRequired { get; set; }

        public OperationBuilder<AddColumnOperation> IsPrivate { get; set; }
    }
}
