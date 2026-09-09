using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldSyncReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_sync_receipt",
                columns: table => new
                {
                    ClientRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RecordType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_sync_receipt", x => x.ClientRecordId);
                });

            migrationBuilder.UpdateData(
                table: "hospital_settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "FacilityName",
                value: "MedReach Hospital");

            migrationBuilder.CreateIndex(
                name: "IX_field_sync_receipt_DeviceId_RecordType",
                table: "field_sync_receipt",
                columns: new[] { "DeviceId", "RecordType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_sync_receipt");

            migrationBuilder.UpdateData(
                table: "hospital_settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "FacilityName",
                value: "Renaissance Hospital");
        }
    }
}
