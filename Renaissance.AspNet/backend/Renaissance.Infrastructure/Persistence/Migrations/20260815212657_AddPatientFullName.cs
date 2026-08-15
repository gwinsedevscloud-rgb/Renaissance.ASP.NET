using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "patient",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE patient
                SET FullName = CASE
                    WHEN CHARINDEX(',', Address) > 0 THEN LTRIM(RTRIM(LEFT(Address, CHARINDEX(',', Address) - 1)))
                    ELSE LTRIM(RTRIM(ISNULL(Address, '')))
                END
                WHERE FullName = ''
                  AND Address IS NOT NULL
                  AND LTRIM(RTRIM(Address)) <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "patient");
        }
    }
}
