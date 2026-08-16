using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hospital_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClientNumberPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WebAccessUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiAccessUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospital_settings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "hospital_settings",
                columns: new[] { "Id", "ApiAccessUrl", "ClientNumberPrefix", "FacilityName", "TimeZoneId", "UpdatedAtUtc", "UpdatedBy", "WebAccessUrl" },
                values: new object[] { 1, "", "ACH", "Renaissance Hospital", "UTC", null, null, "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hospital_settings");
        }
    }
}
