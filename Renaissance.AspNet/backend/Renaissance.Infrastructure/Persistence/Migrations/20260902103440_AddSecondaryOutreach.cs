using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryOutreach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgramType",
                table: "care_program",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "care_program_staff",
                columns: table => new
                {
                    CareProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_care_program_staff", x => new { x.CareProgramId, x.UserId });
                    table.ForeignKey(
                        name: "FK_care_program_staff_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_care_program_staff_care_program_CareProgramId",
                        column: x => x.CareProgramId,
                        principalTable: "care_program",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "secondary_outreach_registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProgramPatientIdId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secondary_outreach_registration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_secondary_outreach_registration_care_program_CareProgramId",
                        column: x => x.CareProgramId,
                        principalTable: "care_program",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_secondary_outreach_registration_program_patient_id_ProgramPatientIdId",
                        column: x => x.ProgramPatientIdId,
                        principalTable: "program_patient_id",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_care_program_staff_UserId",
                table: "care_program_staff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_secondary_outreach_registration_CareProgramId",
                table: "secondary_outreach_registration",
                column: "CareProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_secondary_outreach_registration_ProgramPatientIdId",
                table: "secondary_outreach_registration",
                column: "ProgramPatientIdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "care_program_staff");

            migrationBuilder.DropTable(
                name: "secondary_outreach_registration");

            migrationBuilder.DropColumn(
                name: "ProgramType",
                table: "care_program");
        }
    }
}
