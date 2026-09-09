using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_device",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DeviceLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FirstSeenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPullUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPushUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_device", x => x.DeviceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_device");
        }
    }
}
