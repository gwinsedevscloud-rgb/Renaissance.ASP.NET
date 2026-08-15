using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceReferrals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ren_referral_TargetModule_Status_CreatedDate",
                table: "ren_referral");

            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "ren_referral",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "ren_referral",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ren_referral",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "referral_notification_read",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_notification_read", x => new { x.UserId, x.ReferralId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ren_referral_TargetModule_Status_Priority_CreatedDate",
                table: "ren_referral",
                columns: new[] { "TargetModule", "Status", "Priority", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_notification_read");

            migrationBuilder.DropIndex(
                name: "IX_ren_referral_TargetModule_Status_Priority_CreatedDate",
                table: "ren_referral");

            migrationBuilder.DropColumn(
                name: "CancelledReason",
                table: "ren_referral");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "ren_referral");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ren_referral");

            migrationBuilder.CreateIndex(
                name: "IX_ren_referral_TargetModule_Status_CreatedDate",
                table: "ren_referral",
                columns: new[] { "TargetModule", "Status", "CreatedDate" });
        }
    }
}
