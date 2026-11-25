using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        private static readonly Guid DefaultTenantId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patient_ClientNumber",
                table: "patient");

            migrationBuilder.DropIndex(
                name: "IX_app_user_UserName",
                table: "app_user");

            migrationBuilder.CreateTable(
                name: "tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FacilityName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "tenant",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedBy", "FacilityName", "IsActive", "Name" },
                values: new object[] { DefaultTenantId, "medreach", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "MedReach", true, "MedReach" });

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "patient",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: DefaultTenantId);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "hospital_settings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: DefaultTenantId);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "care_program",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: DefaultTenantId);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "app_user",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql($"UPDATE patient SET TenantId = '{DefaultTenantId}' WHERE TenantId = '00000000-0000-0000-0000-000000000000'");
            migrationBuilder.Sql($"UPDATE care_program SET TenantId = '{DefaultTenantId}' WHERE TenantId = '00000000-0000-0000-0000-000000000000'");
            migrationBuilder.Sql($"UPDATE hospital_settings SET TenantId = '{DefaultTenantId}' WHERE TenantId = '00000000-0000-0000-0000-000000000000'");
            migrationBuilder.Sql($"UPDATE app_user SET TenantId = '{DefaultTenantId}' WHERE TenantId IS NULL");

            migrationBuilder.UpdateData(
                table: "hospital_settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: DefaultTenantId);

            migrationBuilder.CreateIndex(
                name: "IX_patient_TenantId",
                table: "patient",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_TenantId_ClientNumber",
                table: "patient",
                columns: new[] { "TenantId", "ClientNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hospital_settings_TenantId",
                table: "hospital_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_care_program_TenantId",
                table: "care_program",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_TenantId_UserName",
                table: "app_user",
                columns: new[] { "TenantId", "UserName" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_Platform_UserName",
                table: "app_user",
                column: "UserName",
                unique: true,
                filter: "[TenantId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_Code",
                table: "tenant",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_tenant_TenantId",
                table: "app_user",
                column: "TenantId",
                principalTable: "tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hospital_settings_tenant_TenantId",
                table: "hospital_settings",
                column: "TenantId",
                principalTable: "tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_user_tenant_TenantId",
                table: "app_user");

            migrationBuilder.DropForeignKey(
                name: "FK_hospital_settings_tenant_TenantId",
                table: "hospital_settings");

            migrationBuilder.DropTable(
                name: "tenant");

            migrationBuilder.DropIndex(
                name: "IX_patient_TenantId",
                table: "patient");

            migrationBuilder.DropIndex(
                name: "IX_patient_TenantId_ClientNumber",
                table: "patient");

            migrationBuilder.DropIndex(
                name: "IX_hospital_settings_TenantId",
                table: "hospital_settings");

            migrationBuilder.DropIndex(
                name: "IX_care_program_TenantId",
                table: "care_program");

            migrationBuilder.DropIndex(
                name: "IX_app_user_TenantId_UserName",
                table: "app_user");

            migrationBuilder.DropIndex(
                name: "IX_app_user_Platform_UserName",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "patient");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "hospital_settings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "care_program");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "app_user");

            migrationBuilder.CreateIndex(
                name: "IX_patient_ClientNumber",
                table: "patient",
                column: "ClientNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_user_UserName",
                table: "app_user",
                column: "UserName",
                unique: true);
        }
    }
}
