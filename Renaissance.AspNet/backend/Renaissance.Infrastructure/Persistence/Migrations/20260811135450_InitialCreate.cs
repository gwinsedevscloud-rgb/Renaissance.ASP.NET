using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renaissance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_number_sequence",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LastSequence = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_number_sequence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AgeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tribe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ancillary_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Services = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PregnancyStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ancillary_service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ancillary_service_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consultation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnoses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServicesReferred = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersTreatment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Referred = table.Column<bool>(type: "bit", nullable: true),
                    ItnOrder = table.Column<bool>(type: "bit", nullable: true),
                    ItnDispense = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_consultation_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dental_consultation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnoses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispensedItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServicesReferred = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersTreatment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Referred = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dental_consultation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dental_consultation_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ren_laboratory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ren_laboratory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ren_laboratory_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ren_ophthalmologist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnoses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surgeries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OthersTreatment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtherSurgery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisualAcuityRight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisualAcuityLeft = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlassesDispensed = table.Column<bool>(type: "bit", nullable: true),
                    Referred = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ren_ophthalmologist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ren_ophthalmologist_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ren_optometrist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisualAcuityRight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisualAcuityLeft = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlassesDispensed = table.Column<bool>(type: "bit", nullable: true),
                    Referred = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ren_optometrist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ren_optometrist_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ren_pharmacy_prescription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dispensed = table.Column<bool>(type: "bit", nullable: false),
                    DispensationNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    QuantityDispensed = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ren_pharmacy_prescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ren_pharmacy_prescription_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ren_triage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diabetes = table.Column<bool>(type: "bit", nullable: true),
                    Asthma = table.Column<bool>(type: "bit", nullable: true),
                    SickleCell = table.Column<bool>(type: "bit", nullable: true),
                    Smoking = table.Column<bool>(type: "bit", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    SystolicBp = table.Column<int>(type: "int", nullable: true),
                    DiastolicBp = table.Column<int>(type: "int", nullable: true),
                    PulseRate = table.Column<int>(type: "int", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ren_triage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ren_triage_patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "client_number_sequence",
                columns: new[] { "Id", "LastSequence", "Version" },
                values: new object[] { 1L, 0L, 0L });

            migrationBuilder.CreateIndex(
                name: "IX_ancillary_service_PatientId",
                table: "ancillary_service",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_consultation_PatientId",
                table: "consultation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_dental_consultation_PatientId",
                table: "dental_consultation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_ClientNumber",
                table: "patient",
                column: "ClientNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ren_laboratory_PatientId",
                table: "ren_laboratory",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ren_ophthalmologist_PatientId",
                table: "ren_ophthalmologist",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ren_optometrist_PatientId",
                table: "ren_optometrist",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ren_pharmacy_prescription_PatientId",
                table: "ren_pharmacy_prescription",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ren_triage_PatientId",
                table: "ren_triage",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ancillary_service");

            migrationBuilder.DropTable(
                name: "client_number_sequence");

            migrationBuilder.DropTable(
                name: "consultation");

            migrationBuilder.DropTable(
                name: "dental_consultation");

            migrationBuilder.DropTable(
                name: "ren_laboratory");

            migrationBuilder.DropTable(
                name: "ren_ophthalmologist");

            migrationBuilder.DropTable(
                name: "ren_optometrist");

            migrationBuilder.DropTable(
                name: "ren_pharmacy_prescription");

            migrationBuilder.DropTable(
                name: "ren_triage");

            migrationBuilder.DropTable(
                name: "patient");
        }
    }
}
