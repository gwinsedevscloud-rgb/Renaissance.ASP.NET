using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLanAccessPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanAccessPasswordHash",
                table: "hospital_settings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "hospital_settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanAccessPasswordHash",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanAccessPasswordHash",
                table: "hospital_settings");
        }
    }
}
