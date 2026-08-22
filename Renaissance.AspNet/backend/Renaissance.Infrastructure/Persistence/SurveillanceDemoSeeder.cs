using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

/// <summary>
/// Ensures priority surveillance demo records exist even when the main sample seed already ran.
/// </summary>
public static class SurveillanceDemoSeeder
{
    public const string DemoMarker = "demo-surveillance";

    public static async Task EnsureAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Laboratories.AsNoTracking().AnyAsync(l => l.CreatedBy == DemoMarker, cancellationToken))
        {
            logger.LogInformation("Surveillance demo data already present.");
            return;
        }

        var patients = await db.Patients.AsNoTracking()
            .Where(p => !p.Archived)
            .OrderBy(p => p.CreatedDate)
            .Take(8)
            .ToListAsync(cancellationToken);

        if (patients.Count == 0)
        {
            logger.LogWarning("Surveillance demo skipped — no patients in database.");
            return;
        }

        logger.LogInformation("Seeding surveillance demo lab, pharmacy, and antenatal records...");
        var now = DateTime.UtcNow;
        var labs = new List<Laboratory>();
        var ancillaries = new List<Ancillary>();
        var pharmacy = new List<PharmacyPrescription>();

        AssignPatientDemo(patients, now, labs, ancillaries, pharmacy);

        db.Laboratories.AddRange(labs);
        db.Ancillaries.AddRange(ancillaries);
        db.PharmacyPrescriptions.AddRange(pharmacy);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Seeded surveillance demo: {LabCount} lab tests, {AncillaryCount} antenatal records, {PharmacyCount} prescriptions.",
            labs.Count,
            ancillaries.Count,
            pharmacy.Count);
    }

    private static void AssignPatientDemo(
        IReadOnlyList<Patient> patients,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> ancillaries,
        List<PharmacyPrescription> pharmacy)
    {
        var slots = new (int Index, Action<Patient, DateTime, List<Laboratory>, List<Ancillary>, List<PharmacyPrescription>> Seed)[]
        {
            (0, SeedMalariaPositive),
            (1, SeedHivPositive),
            (2, SeedPregnantWoman),
            (3, SeedTbPositive),
            (4, SeedPregnantTeen),
            (5, SeedMixedNegative),
            (6, SeedMalariaAndRespiratory),
            (7, SeedHivNegativeControl)
        };

        foreach (var (index, seed) in slots)
        {
            if (index >= patients.Count)
            {
                break;
            }

            seed(patients[index], now, labs, ancillaries, pharmacy);
        }
    }

    private static void SeedMalariaPositive(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-4), "Malaria Parasite (MP)", "Positive", "Ring forms seen on thick film.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-4), "HIV Rapid Test", "Non-reactive", "Screening negative.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-3), "Antimalarial", "Artemether/Lumefantrine (Coartem)", "80/480 mg", "Twice daily", "3 days",
            DispensationStatus.DISPENSED, true, 6));
    }

    private static void SeedHivPositive(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-6), "HIV Rapid Antibody Test", "Reactive (Positive)", "Confirmatory testing advised.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-5), "CD4 Count", "412 cells/µL", "Immunology follow-up required.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-5), "Malaria RDT", "Negative", "No parasitaemia detected.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-4), "Antiretroviral", "TDF/3TC/DTG (TLD)", "One tablet", "Once daily", "Continuous",
            DispensationStatus.DISPENSED, true, 30));
    }

    private static void SeedPregnantWoman(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> ancillaries,
        List<PharmacyPrescription> pharmacy)
    {
        ancillaries.Add(Ancillary(patient, now.AddDays(-8), ["Antenatal booking", "Nutrition counselling"], "Pregnant — second trimester (24 weeks)"));
        labs.Add(Lab(patient, now.AddDays(-7), "Haemoglobin (Hb)", "10.6 g/dL", "Mild anaemia in pregnancy.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-7), "HIV Rapid Test", "Non-reactive", "Antenatal screening negative.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-7), "Syphilis RPR", "Non-reactive", "No active syphilis detected.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-6), "Urinalysis", "Protein trace", "Repeat at next antenatal visit.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-6), "Supplement", "Ferrous Sulphate + Folic Acid", "200/5 mg", "Once daily", "90 days",
            DispensationStatus.DISPENSED, true, 90));
        pharmacy.Add(Rx(patient, now.AddDays(-6), "Antenatal", "Tetanus Toxoid", "0.5 mL IM", "Single dose", "Once",
            DispensationStatus.DISPENSED, true, 1));
    }

    private static void SeedTbPositive(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-3), "TB GeneXpert MTB/RIF", "MTB Detected, RIF Sensitive", "Positive — initiate TB treatment.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-2), "Sputum AFB Smear", "Positive (2+)", "Acid-fast bacilli seen.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-2), "HIV Rapid Test", "Non-reactive", "Co-infection screening negative.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-1), "Antitubercular", "RHZE (4FDC)", "Fixed-dose combination", "Once daily", "2 months (intensive phase)",
            DispensationStatus.PENDING, false, null));
    }

    private static void SeedPregnantTeen(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> ancillaries,
        List<PharmacyPrescription> pharmacy)
    {
        ancillaries.Add(Ancillary(patient, now.AddDays(-5), ["Antenatal counselling", "Adolescent pregnancy support"], "Pregnant — first trimester (12 weeks)"));
        labs.Add(Lab(patient, now.AddDays(-4), "Malaria Parasite (MP)", "Positive", "Uncomplicated malaria in pregnancy.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-4), "Haemoglobin (Hb)", "9.8 g/dL", "Moderate anaemia — treat and monitor.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-3), "HIV Rapid Test", "Non-reactive", "Antenatal HIV screening negative.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-2), "Antimalarial", "Artesunate-Amodiaquine", "100/270 mg", "Once daily", "3 days",
            DispensationStatus.DISPENSED, true, 3));
        pharmacy.Add(Rx(patient, now.AddDays(-2), "Supplement", "Folic Acid", "5 mg", "Once daily", "30 days",
            DispensationStatus.DISPENSED, true, 30));
    }

    private static void SeedMixedNegative(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-2), "Malaria RDT", "Negative", "No malaria parasitaemia.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-2), "HIV Rapid Test", "Non-reactive", "Routine screening negative.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-1), "Fasting Blood Sugar", "5.4 mmol/L", "Within acceptable range.", DemoMarker));
        pharmacy.Add(Rx(patient, now.AddDays(-1), "Antihypertensive", "Amlodipine", "5 mg", "Once daily", "30 days",
            DispensationStatus.PENDING, false, null));
    }

    private static void SeedMalariaAndRespiratory(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-1), "Malaria Parasite (MP)", "Positive", "Uncomplicated P. falciparum.", DemoMarker));
        labs.Add(Lab(patient, now.AddDays(-1), "Full Blood Count", "Within normal limits", "No leucocytosis.", DemoMarker));
        pharmacy.Add(Rx(patient, now, "Antimalarial", "Artemether/Lumefantrine (Coartem)", "80/480 mg", "Twice daily", "3 days",
            DispensationStatus.DISPENSED, true, 6));
        pharmacy.Add(Rx(patient, now, "Analgesic", "Paracetamol", "500 mg", "Three times daily", "5 days",
            DispensationStatus.DISPENSED, true, 15));
    }

    private static void SeedHivNegativeControl(
        Patient patient,
        DateTime now,
        List<Laboratory> labs,
        List<Ancillary> _,
        List<PharmacyPrescription> pharmacy)
    {
        labs.Add(Lab(patient, now.AddDays(-1), "HIV Rapid Test", "Non-reactive", "Annual screening negative.", DemoMarker));
        labs.Add(Lab(patient, now, "TB GeneXpert MTB/RIF", "MTB Not Detected", "No tuberculosis detected.", DemoMarker));
        pharmacy.Add(Rx(patient, now, "Antibiotic", "Amoxicillin", "500 mg", "Three times daily", "7 days",
            DispensationStatus.DISPENSED, true, 21));
    }

    private static Laboratory Lab(Patient patient, DateTime when, string test, string result, string note, string marker)
    {
        var lab = new Laboratory
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            TestName = test,
            Result = result,
            Note = $"{note} [{marker}]"
        };
        Stamp(lab, when);
        return lab;
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

    private static PharmacyPrescription Rx(
        Patient patient,
        DateTime when,
        string category,
        string drug,
        string dosage,
        string frequency,
        string duration,
        DispensationStatus status,
        bool dispensed,
        int? quantity)
    {
        var rx = new PharmacyPrescription
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            DrugCategory = category,
            DrugName = drug,
            Dosage = dosage,
            Frequency = frequency,
            Duration = duration,
            Status = status,
            Dispensed = dispensed,
            QuantityDispensed = quantity
        };
        Stamp(rx, when);
        return rx;
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
