using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ProfileEntityBuilder : AuditableBaseEntityBuilder<ProfileEntityBuilder>
    {
        private const string _entityTableName = "Profile";
        private readonly PrimaryKey<ProfileEntityBuilder> _primaryKey = new("PK_Profile", x => x.ProfileId);
        private readonly ForeignKey<ProfileEntityBuilder> _siteForeignKey = new("FK_Profile_Sites", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public ProfileEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override ProfileEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ProfileId = ActiveDatabase.AddAutoIncrementColumn(table,"ProfileId");
            SiteId = table.AddIntegerColumn("SiteId", true);
            Name = table.AddStringColumn("Name", 50);
            Title = table.AddStringColumn("Title", 50);
            Description = table.AddStringColumn("Description", 256, true);
            Category = table.AddStringColumn("Category", 50);
            ViewOrder = table.AddIntegerColumn("ViewOrder");
            MaxLength = table.AddIntegerColumn("MaxLength");
            DefaultValue = table.AddStringColumn("DefaultValue", 2000, true);
            IsRequired = table.AddBooleanColumn("IsRequired");
            IsPrivate = table.AddBooleanColumn("IsPrivate");

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
