using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

public static class SecondaryOutreachSeeder
{
    public static readonly Guid PatientDewormingProgramId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid ItnProgramId = Guid.Parse("bbbbbbbb-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid DentalClinicProgramId = Guid.Parse("cccccccc-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid EyeClinicProgramId = Guid.Parse("dddddddd-bbbb-cccc-dddd-eeeeeeeeeeee");

    public static async Task EnsureAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        await EnsureProgramAsync(db, logger, PatientDewormingProgramId, CreateDewormingTemplate, cancellationToken);
        await EnsureProgramAsync(db, logger, ItnProgramId, CreateItnTemplate, cancellationToken);
        await EnsureProgramAsync(db, logger, DentalClinicProgramId, CreateDentalClinicTemplate, cancellationToken);
        await EnsureProgramAsync(db, logger, EyeClinicProgramId, CreateEyeClinicTemplate, cancellationToken);
        await SyncLinkedModulesAsync(db, cancellationToken);
    }

    private static async Task EnsureProgramAsync(
        RenaissanceDbContext db,
        ILogger logger,
        Guid id,
        Func<DateTime, CareProgram> factory,
        CancellationToken cancellationToken)
    {
        if (await db.CarePrograms.AnyAsync(p => p.Id == id, cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var program = factory(now);
        db.CarePrograms.Add(program);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded secondary outreach program: {Name} (draft).", program.Name);
    }

    private static async Task SyncLinkedModulesAsync(RenaissanceDbContext db, CancellationToken cancellationToken)
    {
        var dental = await db.CarePrograms.FirstOrDefaultAsync(p => p.Id == DentalClinicProgramId, cancellationToken);
        if (dental is not null && dental.LinkedClinicalModule != AppModule.Dental)
        {
            dental.LinkedClinicalModule = AppModule.Dental;
        }

        var eye = await db.CarePrograms.FirstOrDefaultAsync(p => p.Id == EyeClinicProgramId, cancellationToken);
        if (eye is not null && eye.LinkedClinicalModule != AppModule.Optometrists)
        {
            eye.LinkedClinicalModule = AppModule.Optometrists;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static CareProgram CreateDewormingTemplate(DateTime now) => new()
    {
        Id = PatientDewormingProgramId,
        Name = "Patient Deworming",
        ProgramType = CareProgramType.Secondary,
        StartDate = now.Date,
        EndDate = now.Date.AddMonths(3),
        TargetAudience = "School-age children and community members",
        TargetAgeGroup = "5–17 years",
        TargetCommunity = "Local community",
        TargetedTreatment = "Deworming",
        Status = CareProgramStatus.Draft,
        PatientIdMode = PatientIdMode.AutoSerial,
        OutreachCode = "DEWORM",
        SerialPadding = 4,
        SerialStartNumber = 1,
        NextSerialNumber = 1,
        CreatedBy = "seed",
        CreatedDate = now,
        UpdatedBy = "seed",
        UpdatedDate = now
    };

    private static CareProgram CreateItnTemplate(DateTime now) => new()
    {
        Id = ItnProgramId,
        Name = "Insecticide Treated Net (ITN)",
        ProgramType = CareProgramType.Secondary,
        StartDate = now.Date,
        EndDate = now.Date.AddMonths(3),
        TargetAudience = "Households and pregnant women",
        TargetAgeGroup = "All ages",
        TargetCommunity = "Local community",
        TargetedTreatment = "Insecticide-treated net distribution",
        Status = CareProgramStatus.Draft,
        PatientIdMode = PatientIdMode.AutoSerial,
        OutreachCode = "ITN",
        SerialPadding = 4,
        SerialStartNumber = 1,
        NextSerialNumber = 1,
        CreatedBy = "seed",
        CreatedDate = now,
        UpdatedBy = "seed",
        UpdatedDate = now
    };

    private static CareProgram CreateDentalClinicTemplate(DateTime now) => new()
    {
        Id = DentalClinicProgramId,
        Name = "Dental Clinic",
        ProgramType = CareProgramType.Secondary,
        LinkedClinicalModule = AppModule.Dental,
        StartDate = now.Date,
        EndDate = now.Date.AddMonths(6),
        TargetAudience = "Primary outreach patients referred from triage",
        TargetAgeGroup = "All ages",
        TargetCommunity = "Outreach site",
        TargetedTreatment = "Dental services (check-up, preventive, restorative, oral surgery)",
        Status = CareProgramStatus.Draft,
        PatientIdMode = PatientIdMode.AutoSerial,
        OutreachCode = "DENTAL",
        SerialPadding = 4,
        SerialStartNumber = 1,
        NextSerialNumber = 1,
        CreatedBy = "seed",
        CreatedDate = now,
        UpdatedBy = "seed",
        UpdatedDate = now
    };

    private static CareProgram CreateEyeClinicTemplate(DateTime now) => new()
    {
        Id = EyeClinicProgramId,
        Name = "Eye Clinic",
        ProgramType = CareProgramType.Secondary,
        LinkedClinicalModule = AppModule.Optometrists,
        StartDate = now.Date,
        EndDate = now.Date.AddMonths(6),
        TargetAudience = "Primary outreach patients referred from triage",
        TargetAgeGroup = "All ages",
        TargetCommunity = "Outreach site",
        TargetedTreatment = "Eye health screening, refraction, and minor ocular care",
        Status = CareProgramStatus.Draft,
        PatientIdMode = PatientIdMode.AutoSerial,
        OutreachCode = "EYE",
        SerialPadding = 4,
        SerialStartNumber = 1,
        NextSerialNumber = 1,
        CreatedBy = "seed",
        CreatedDate = now,
        UpdatedBy = "seed",
        UpdatedDate = now
    };
}
