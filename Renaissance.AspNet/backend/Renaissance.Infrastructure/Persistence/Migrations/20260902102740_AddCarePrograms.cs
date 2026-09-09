using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCarePrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "patient",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "patient",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CareProgramId",
                table: "patient",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OutreachModuleEnabled",
                table: "hospital_settings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "care_program",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TargetAgeGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetCommunity = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TargetedTreatment = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientIdMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OutreachCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    SerialPadding = table.Column<int>(type: "int", nullable: false),
                    SerialStartNumber = table.Column<int>(type: "int", nullable: false),
                    BatchSize = table.Column<int>(type: "int", nullable: true),
                    NextSerialNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_care_program", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "program_patient_id",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegisteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_program_patient_id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_program_patient_id_care_program_CareProgramId",
                        column: x => x.CareProgramId,
                        principalTable: "care_program",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_program_patient_id_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "hospital_settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "OutreachModuleEnabled",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_CareProgramId",
                table: "patient",
                column: "CareProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_care_program_Status",
                table: "care_program",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_program_patient_id_CareProgramId_Code",
                table: "program_patient_id",
                columns: new[] { "CareProgramId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_program_patient_id_CareProgramId_Status",
                table: "program_patient_id",
                columns: new[] { "CareProgramId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_program_patient_id_PatientId",
                table: "program_patient_id",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_patient_care_program_CareProgramId",
                table: "patient",
                column: "CareProgramId",
                principalTable: "care_program",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patient_care_program_CareProgramId",
                table: "patient");

            migrationBuilder.DropTable(
                name: "program_patient_id");

            migrationBuilder.DropTable(
                name: "care_program");

            migrationBuilder.DropIndex(
                name: "IX_patient_CareProgramId",
                table: "patient");

            migrationBuilder.DropColumn(
                name: "CareProgramId",
                table: "patient");

            migrationBuilder.DropColumn(
                name: "OutreachModuleEnabled",
                table: "hospital_settings");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "patient",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "patient",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
