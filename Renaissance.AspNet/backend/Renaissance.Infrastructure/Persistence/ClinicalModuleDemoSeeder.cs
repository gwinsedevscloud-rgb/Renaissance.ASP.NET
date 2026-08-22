using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

/// <summary>
/// Ensures demo records exist across triage, consultations, dental, ancillary, optometry, and ophthalmology.
/// </summary>
public static class ClinicalModuleDemoSeeder
{
    public const string DemoMarker = "demo-clinical";

    public static async Task EnsureAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Triages.AsNoTracking().AnyAsync(t => t.CreatedBy == DemoMarker, cancellationToken))
        {
            logger.LogInformation("Clinical module demo data already present.");
            return;
        }

        var patients = await db.Patients.AsNoTracking()
            .Where(p => !p.Archived)
            .OrderBy(p => p.CreatedDate)
            .Take(8)
            .ToListAsync(cancellationToken);

        if (patients.Count == 0)
        {
            logger.LogWarning("Clinical module demo skipped — no patients in database.");
            return;
        }

        logger.LogInformation("Seeding clinical module demo records...");
        var now = DateTime.UtcNow;

        db.Triages.AddRange(BuildTriages(patients, now));
        db.Consultations.AddRange(BuildConsultations(patients, now));
        db.DentalConsultations.AddRange(BuildDental(patients, now));
        db.Ancillaries.AddRange(BuildAncillary(patients, now));
        db.Optometrists.AddRange(BuildOptometrists(patients, now));
        db.Ophthalmologists.AddRange(BuildOphthalmologists(patients, now));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded clinical module demo records for {Count} patients.", patients.Count);
    }

    private static IEnumerable<Triage> BuildTriages(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 0) yield return Triage(patients[0], now.AddHours(-6), 118, 76, 38.2, 96, 22);
        if (patients.Count > 1) yield return Triage(patients[1], now.AddHours(-5), 132, 84, 37.1, 88, 20);
        if (patients.Count > 2) yield return Triage(patients[2], now.AddHours(-4), 98, 62, 36.8, 82, 18);
        if (patients.Count > 3) yield return Triage(patients[3], now.AddHours(-3), 126, 80, 37.4, 90, 21);
        if (patients.Count > 4) yield return Triage(patients[4], now.AddHours(-2), 104, 68, 36.7, 78, 18);
        if (patients.Count > 5) yield return Triage(patients[5], now.AddHours(-1), 142, 88, 36.9, 84, 19);
    }

    private static IEnumerable<Consultation> BuildConsultations(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 0)
        {
            yield return Consultation(patients[0], now.AddHours(-5),
                ["Uncomplicated malaria"], ["Artemether/Lumefantrine", "Oral rehydration therapy"], ["Laboratory", "Pharmacy"], itnOrder: true);
        }

        if (patients.Count > 1)
        {
            yield return Consultation(patients[1], now.AddHours(-4),
                ["Essential hypertension"], ["Amlodipine 5 mg daily", "Low-salt diet counselling"], ["Laboratory", "Pharmacy"]);
        }

        if (patients.Count > 3)
        {
            yield return Consultation(patients[3], now.AddHours(-2),
                ["Pneumonia"], ["Amoxicillin/clavulanate", "Paracetamol and rest"], ["Laboratory", "Pharmacy", "Triage"]);
        }

        if (patients.Count > 5)
        {
            yield return Consultation(patients[5], now.AddHours(-1),
                ["Peptic ulcer disease"], ["Omeprazole", "Diet modification"], ["Pharmacy"]);
        }

        if (patients.Count > 6)
        {
            yield return Consultation(patients[6], now.AddMinutes(-30),
                ["Typhoid fever"], ["Ceftriaxone IM", "Oral rehydration therapy"], ["Laboratory", "Pharmacy"]);
        }
    }

    private static IEnumerable<DentalConsultation> BuildDental(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 3)
        {
            yield return Dental(patients[3], now.AddHours(-2),
                ["Dental caries — lower molar"], ["Extraction planned"], ["Tooth extraction", "Ibuprofen 400 mg"]);
        }

        if (patients.Count > 5)
        {
            yield return Dental(patients[5], now.AddHours(-1),
                ["Gingivitis"], ["Scaling and polishing"], ["Chlorhexidine mouthwash"]);
        }

        if (patients.Count > 1)
        {
            yield return Dental(patients[1], now.AddMinutes(-45),
                ["Pericoronitis"], ["Incision and drainage", "Analgesia and review"], ["Amoxicillin 500 mg"]);
        }
    }

    private static IEnumerable<Ancillary> BuildAncillary(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 6)
        {
            yield return Ancillary(patients[6], now.AddHours(-2),
                ["Family planning counselling", "Health talk — hygiene"], "Not applicable");
        }

        if (patients.Count > 0)
        {
            yield return Ancillary(patients[0], now.AddHours(-3),
                ["HIV prevention counselling"], "Not applicable");
        }

        if (patients.Count > 4)
        {
            yield return Ancillary(patients[4], now.AddHours(-1),
                ["Adolescent health talk"], "Not applicable");
        }
    }

    private static IEnumerable<Optometrist> BuildOptometrists(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 4)
        {
            yield return Opto(patients[4], now.AddHours(-2), "6/24", "6/18", glasses: true, referred: false);
        }

        if (patients.Count > 0)
        {
            yield return Opto(patients[0], now.AddHours(-1), "6/12", "6/12", glasses: false, referred: false);
        }

        if (patients.Count > 2)
        {
            yield return Opto(patients[2], now.AddMinutes(-40), "6/18", "6/18", glasses: true, referred: true);
        }
    }

    private static IEnumerable<Ophthalmologist> BuildOphthalmologists(IReadOnlyList<Patient> patients, DateTime now)
    {
        if (patients.Count > 5)
        {
            yield return Ophth(patients[5], now.AddHours(-1),
                ["Immature cataract — OS"], ["Phacoemulsification recommended"],
                ["Cataract surgery — left eye"], "3/60", "6/18", referred: true);
        }

        if (patients.Count > 1)
        {
            yield return Ophth(patients[1], now.AddMinutes(-50),
                ["Primary open-angle glaucoma"], ["Timolol eye drops"],
                [], "6/18", "6/24", referred: false);
        }

        if (patients.Count > 4)
        {
            yield return Ophth(patients[4], now.AddMinutes(-20),
                ["Allergic conjunctivitis"], ["Artificial tears", "Observation and review"],
                [], "6/9", "6/9", referred: false);
        }
    }

    private static Triage Triage(
        Patient patient,
        DateTime when,
        int systolic,
        int diastolic,
        double temp,
        int pulse,
        int resp)
    {
        var triage = new Triage
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            SystolicBp = systolic,
            DiastolicBp = diastolic,
            Temperature = temp,
            PulseRate = pulse,
            RespiratoryRate = resp,
            Weight = 58 + patient.Age % 12,
            Height = 158 + (patient.Age % 8),
            Diabetes = patient.Age > 45,
            Asthma = false,
            SickleCell = patient.Age < 35,
            Smoking = false
        };
        Stamp(triage, when);
        return triage;
    }

    private static Consultation Consultation(
        Patient patient,
        DateTime when,
        List<string> diagnoses,
        List<string> treatments,
        List<string> referredServices,
        bool? itnOrder = null)
    {
        var consultation = new Consultation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Diagnoses = diagnoses,
            Treatments = treatments,
            ServicesReferred = referredServices,
            Referred = referredServices.Count > 0,
            ItnOrder = itnOrder ?? false,
            ItnDispense = false
        };
        Stamp(consultation, when);
        return consultation;
    }

    private static DentalConsultation Dental(
        Patient patient,
        DateTime when,
        List<string> diagnoses,
        List<string> treatments,
        List<string> dispensed)
    {
        var dental = new DentalConsultation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Diagnoses = diagnoses,
            Treatments = treatments,
            DispensedItems = dispensed
        };
        Stamp(dental, when);
        return dental;
    }

    private static Ancillary Ancillary(Patient patient, DateTime when, List<string> services, string pregnancyStatus)
    {
        var ancillary = new Ancillary
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Services = services,
            PregnancyStatus = pregnancyStatus
        };
        Stamp(ancillary, when);
        return ancillary;
    }

    private static Optometrist Opto(
        Patient patient,
        DateTime when,
        string right,
        string left,
        bool glasses,
        bool referred)
    {
        var opto = new Optometrist
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            VisualAcuityRight = right,
            VisualAcuityLeft = left,
            GlassesDispensed = glasses,
            Referred = referred
        };
        Stamp(opto, when);
        return opto;
    }

    private static Ophthalmologist Ophth(
        Patient patient,
        DateTime when,
        List<string> diagnoses,
        List<string> treatments,
        List<string> surgeries,
        string right,
        string left,
        bool referred)
    {
        var ophth = new Ophthalmologist
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Diagnoses = diagnoses,
            Treatments = treatments,
            Surgeries = surgeries,
            VisualAcuityRight = right,
            VisualAcuityLeft = left,
            GlassesDispensed = false,
            Referred = referred
        };
        Stamp(ophth, when);
        return ophth;
    }

    private static void Stamp(AuditEntity entity, DateTime created)
    {
        entity.CreatedBy = DemoMarker;
        entity.CreatedDate = created;
        entity.UpdatedBy = DemoMarker;
        entity.UpdatedDate = created;
        entity.Archived = false;
    }
}
