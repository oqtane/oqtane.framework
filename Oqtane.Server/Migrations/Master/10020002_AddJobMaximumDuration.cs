using System.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.10.02.00.02")]
    public class AddJobMaximumDuration : MultiDatabaseMigration
    {
        public AddJobMaximumDuration(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.AddIntegerColumn("MaximumDuration", true);
            jobEntityBuilder.UpdateData("MaximumDuration", 30);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
