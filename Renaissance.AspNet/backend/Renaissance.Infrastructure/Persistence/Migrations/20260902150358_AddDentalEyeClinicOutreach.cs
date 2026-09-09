using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDentalEyeClinicOutreach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Medications",
                table: "ren_optometrist",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Services",
                table: "ren_optometrist",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedClinicalModule",
                table: "care_program",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Medications",
                table: "ren_optometrist");

            migrationBuilder.DropColumn(
                name: "Services",
                table: "ren_optometrist");

            migrationBuilder.DropColumn(
                name: "LinkedClinicalModule",
                table: "care_program");
        }
    }
}
