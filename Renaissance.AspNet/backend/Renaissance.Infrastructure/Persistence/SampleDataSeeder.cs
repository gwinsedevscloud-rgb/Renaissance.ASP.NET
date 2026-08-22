using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

public static class SampleDataSeeder
{
    public static async Task SeedAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Patients.AsNoTracking().AnyAsync(cancellationToken))
        {
            logger.LogInformation("Sample data skipped — patients already exist.");
            return;
        }

        logger.LogInformation("Seeding Akwa Ibom sample clinical data...");

        var now = DateTime.UtcNow;
        var patients = BuildPatients(now);
        db.Patients.AddRange(patients);

        db.Triages.AddRange(BuildTriages(patients, now));
        db.Consultations.AddRange(BuildConsultations(patients, now));
        db.Laboratories.AddRange(BuildLaboratories(patients, now));
        db.PharmacyPrescriptions.AddRange(BuildPharmacy(patients, now));
        db.DentalConsultations.AddRange(BuildDental(patients, now));
        db.Ancillaries.AddRange(BuildAncillary(patients, now));
        db.Optometrists.AddRange(BuildOptometrists(patients, now));
        db.Ophthalmologists.AddRange(BuildOphthalmologists(patients, now));

        var sequence = await db.ClientNumberSequences.FirstAsync(x => x.Id == 1, cancellationToken);
        sequence.LastSequence = patients.Count;
        sequence.Version += 1;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} Akwa Ibom sample clients with clinical records.", patients.Count);
    }

    private static List<Patient> BuildPatients(DateTime now)
    {
        return
        [
            Patient("Okon Bassey", 1, "Male", 45, "Ibibio", "Uyo", "Married", "Farmer", "Primary", "+2348032104501", now.AddDays(-26)),
            Patient("Etim Akpan", 2, "Male", 32, "Annang", "Eket", "Single", "Fisherman", "Secondary", "+2348054412890", now.AddDays(-20)),
            Patient("Nkoyo Udo", 3, "Female", 28, "Ibibio", "Ikot Ekpene", "Married", "Market Trader", "Secondary", "+2348079923411", now.AddDays(-15)),
            Patient("Ita Ekpo", 4, "Male", 55, "Oron", "Oron", "Married", "Civil Servant", "Tertiary", "+2348027789012", now.AddDays(-11)),
            Patient("Mmeyene John", 5, "Female", 19, "Ibibio", "Uyo", "Single", "Student", "Secondary", "+2348093345678", now.AddDays(-7)),
            Patient("Bassey Effiong", 6, "Male", 62, "Efik", "Ikot Abasi", "Married", "Retired Teacher", "Tertiary", "+2348061123490", now.AddDays(-4)),
            Patient("Etieno Sunday", 7, "Male", 38, "Ibibio", "Mkpat Enin", "Married", "Commercial Driver", "Secondary", "+2348087654321", now.AddDays(-2))
        ];
    }

    private static Patient Patient(
        string fullName,
        int sequence,
        string sex,
        int age,
        string tribe,
        string lga,
        string maritalStatus,
        string occupation,
        string education,
        string phone,
        DateTime registered)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClientNumber = $"ACH{sequence:D4}",
            FullName = fullName,
            Sex = sex,
            Age = age,
            AgeUnit = "Years",
            Tribe = tribe,
            Religion = "Christian",
            MaritalStatus = maritalStatus,
            Occupation = occupation,
            Education = education,
            PhoneNumber = phone,
            Address = $"{lga} LGA, Akwa Ibom State"
        };
        Stamp(patient, registered);
        return patient;
    }

    private static IEnumerable<Triage> BuildTriages(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Triage(patients[0], now.AddDays(-25), 78, 68, 36.8, 72, 18, diabetes: false);
        yield return Triage(patients[1], now.AddDays(-19), 88, 92, 36.6, 88, 20, diabetes: false);
        yield return Triage(patients[2], now.AddDays(-14), 62, 58, 36.9, 76, 18, diabetes: false);
        yield return Triage(patients[3], now.AddDays(-10), 82, 70, 37.0, 80, 19);
        yield return Triage(patients[4], now.AddDays(-6), 58, 54, 36.7, 70, 17);
        yield return Triage(patients[5], now.AddDays(-3), 90, 74, 36.5, 84, 18);
        yield return Triage(patients[6], now.AddDays(-1), 76, 66, 36.8, 74, 18);

        yield return Triage(patients[0], now.AddDays(-10), 80, 70, 36.7, 74, 18, diabetes: false);
        yield return Triage(patients[2], now.AddDays(-5), 64, 60, 37.1, 78, 19, diabetes: false);
    }

    private static IEnumerable<Consultation> BuildConsultations(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Consultation(patients[0], now.AddDays(-24),
            ["Uncomplicated malaria"], ["Artemether/Lumefantrine"], ["Laboratory"], itnOrder: true, itnDispense: true);
        yield return Consultation(patients[1], now.AddDays(-18),
            ["Essential hypertension"], ["Amlodipine 5mg daily", "Low-salt diet"], ["Laboratory", "Pharmacy"]);
        yield return Consultation(patients[2], now.AddDays(-13),
            ["Antenatal booking — early pregnancy"], ["Folic acid", "Tetanus toxoid"], ["Ancillary", "Laboratory"]);
        yield return Consultation(patients[3], now.AddDays(-9),
            ["Type 2 diabetes mellitus"], ["Metformin", "Diet counselling"], ["Laboratory", "Dental"]);
        yield return Consultation(patients[4], now.AddDays(-5),
            ["Refractive error — myopia"], ["Corrective lenses"], ["Optometry"]);
        yield return Consultation(patients[5], now.AddDays(-2),
            ["Senile cataract — left eye"], ["Surgical review"], ["Ophthalmology"], referred: true);
        yield return Consultation(patients[6], now.AddDays(-1),
            ["Upper respiratory tract infection"], ["Paracetamol", "Rest and fluids"], ["Pharmacy", "Laboratory"]);
    }

    private static IEnumerable<Laboratory> BuildLaboratories(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Lab(patients[0], now.AddDays(-23), "Malaria Parasite (MP)", "Positive", "Ring forms seen on thick film.");
        yield return Lab(patients[0], now.AddDays(-22), "HIV Rapid Test", "Non-reactive", "Routine screening negative.");
        yield return Lab(patients[1], now.AddDays(-17), "HIV Rapid Antibody Test", "Reactive (Positive)", "Confirmatory testing advised.");
        yield return Lab(patients[1], now.AddDays(-16), "CD4 Count", "398 cells/µL", "Immunology review recommended.");
        yield return Lab(patients[1], now.AddDays(-17), "Lipid Profile", "Borderline high LDL", "Repeat in 3 months advised.");
        yield return Lab(patients[2], now.AddDays(-12), "Haemoglobin (Hb)", "11.2 g/dL", "Mild anaemia in pregnancy.");
        yield return Lab(patients[2], now.AddDays(-12), "HIV Rapid Test", "Non-reactive", "Antenatal screening negative.");
        yield return Lab(patients[2], now.AddDays(-11), "Syphilis RPR", "Non-reactive", "No active syphilis detected.");
        yield return Lab(patients[3], now.AddDays(-8), "Fasting Blood Sugar", "8.4 mmol/L", "Elevated — correlate clinically.");
        yield return Lab(patients[3], now.AddDays(-7), "TB GeneXpert MTB/RIF", "MTB Detected, RIF Sensitive", "Initiate TB regimen.");
        yield return Lab(patients[4], now.AddDays(-5), "Malaria Parasite (MP)", "Positive", "Adolescent antenatal case.");
        yield return Lab(patients[4], now.AddDays(-5), "Haemoglobin (Hb)", "10.1 g/dL", "Anaemia in pregnancy.");
        yield return Lab(patients[5], now.AddDays(-2), "Sputum AFB Smear", "Positive (1+)", "AFB seen — TB treatment started.");
        yield return Lab(patients[6], now.AddDays(-1), "Full Blood Count", "Within normal limits", "No leucocytosis.");
        yield return Lab(patients[6], now.AddDays(-1), "Malaria RDT", "Positive", "Uncomplicated malaria.");
    }

    private static IEnumerable<PharmacyPrescription> BuildPharmacy(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Rx(patients[0], now.AddDays(-22), "Antimalarial", "Artemether/Lumefantrine (Coartem)", "80/480 mg", "Twice daily", "3 days",
            DispensationStatus.DISPENSED, true, 6);
        yield return Rx(patients[1], now.AddDays(-15), "Antiretroviral", "TDF/3TC/DTG (TLD)", "One tablet", "Once daily", "Continuous",
            DispensationStatus.DISPENSED, true, 30);
        yield return Rx(patients[1], now.AddDays(-16), "Antihypertensive", "Amlodipine", "5 mg", "Once daily", "30 days",
            DispensationStatus.PENDING, false, null);
        yield return Rx(patients[2], now.AddDays(-11), "Supplement", "Ferrous Sulphate + Folic Acid", "200/5 mg", "Once daily", "90 days",
            DispensationStatus.DISPENSED, true, 90);
        yield return Rx(patients[2], now.AddDays(-11), "Antenatal", "Tetanus Toxoid", "0.5 mL IM", "Single dose", "Once",
            DispensationStatus.DISPENSED, true, 1);
        yield return Rx(patients[3], now.AddDays(-7), "Antidiabetic", "Metformin", "500 mg", "Twice daily", "30 days",
            DispensationStatus.DISPENSED, true, 60);
        yield return Rx(patients[3], now.AddDays(-6), "Antitubercular", "RHZE (4FDC)", "Fixed-dose combination", "Once daily", "2 months (intensive phase)",
            DispensationStatus.PENDING, false, null);
        yield return Rx(patients[4], now.AddDays(-4), "Antimalarial", "Artesunate-Amodiaquine", "100/270 mg", "Once daily", "3 days",
            DispensationStatus.DISPENSED, true, 3);
        yield return Rx(patients[5], now.AddDays(-2), "Antiglaucoma", "Timolol eye drops", "0.5%", "Twice daily", "30 days",
            DispensationStatus.DECLINED, false, null, "Patient referred for surgery first.");
        yield return Rx(patients[6], now.AddDays(-1), "Analgesic", "Paracetamol", "500 mg", "Three times daily", "5 days",
            DispensationStatus.DISPENSED, true, 15);
        yield return Rx(patients[6], now.AddDays(-1), "Antibiotic", "Amoxicillin", "500 mg", "Three times daily", "7 days",
            DispensationStatus.DISPENSED, true, 21);
        yield return Rx(patients[0], now.AddDays(-10), "Antihypertensive", "Lisinopril", "10 mg", "Once daily", "30 days",
            DispensationStatus.DISPENSED, true, 30);
        yield return Rx(patients[5], now.AddDays(-1), "Antitubercular", "RHZE (4FDC)", "Fixed-dose combination", "Once daily", "2 months (intensive phase)",
            DispensationStatus.DISPENSED, true, 60);
    }

    private static IEnumerable<DentalConsultation> BuildDental(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Dental(patients[3], now.AddDays(-7), ["Dental caries — lower molar"], ["Extraction planned"], ["Tooth extraction"]);
        yield return Dental(patients[1], now.AddDays(-4), ["Gingivitis"], ["Scaling and polishing"], ["Chlorhexidine mouthwash"]);
        yield return Dental(patients[5], now.AddDays(-2), ["Pericoronitis"], ["Incision and drainage"], ["Amoxicillin 500 mg"]);
    }

    private static IEnumerable<Ancillary> BuildAncillary(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Ancillary(patients[2], now.AddDays(-11), ["Antenatal counselling", "Nutrition education"], "Pregnant — first trimester (14 weeks)");
        yield return Ancillary(patients[4], now.AddDays(-5), ["Antenatal booking", "Adolescent pregnancy support"], "Pregnant — first trimester (12 weeks)");
        yield return Ancillary(patients[6], now.AddDays(-1), ["Health talk — road safety"], "Not applicable");
        yield return Ancillary(patients[0], now.AddDays(-3), ["HIV prevention counselling"], "Not applicable");
        yield return Ancillary(patients[6], now.AddDays(-2), ["Family planning counselling"], "Not applicable");
    }

    private static IEnumerable<Optometrist> BuildOptometrists(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Opto(patients[4], now.AddDays(-4), "6/24", "6/18", glasses: true, referred: false);
        yield return Opto(patients[0], now.AddDays(-3), "6/12", "6/12", glasses: false, referred: false);
        yield return Opto(patients[2], now.AddDays(-2), "6/18", "6/18", glasses: true, referred: true);
    }

    private static IEnumerable<Ophthalmologist> BuildOphthalmologists(IReadOnlyList<Patient> patients, DateTime now)
    {
        yield return Ophth(patients[5], now.AddDays(-1), ["Immature cataract — OS"], ["Phacoemulsification recommended"],
            ["Cataract surgery — left eye"], "3/60", "6/18", referred: true);
        yield return Ophth(patients[1], now.AddDays(-3), ["Primary open-angle glaucoma"], ["Timolol eye drops"],
            [], "6/18", "6/24", referred: false);
        yield return Ophth(patients[4], now.AddDays(-2), ["Allergic conjunctivitis"], ["Artificial tears"],
            [], "6/9", "6/9", referred: false);
    }

    private static Triage Triage(
        Patient patient,
        DateTime when,
        int systolic,
        int diastolic,
        double temp,
        int pulse,
        int resp,
        bool? diabetes = null)
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
            Weight = 62 + patient.Age % 15,
            Height = 155 + (patient.Age % 10) * 2,
            Diabetes = diabetes ?? false,
            Asthma = false,
            SickleCell = patient.Tribe == "Ibibio" && patient.Age < 30,
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
        bool? referred = null,
        bool? itnOrder = null,
        bool? itnDispense = null)
    {
        var consultation = new Consultation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Diagnoses = diagnoses,
            Treatments = treatments,
            ServicesReferred = referredServices,
            Referred = referred ?? false,
            ItnOrder = itnOrder ?? false,
            ItnDispense = itnDispense ?? false
        };
        Stamp(consultation, when);
        return consultation;
    }

    private static Laboratory Lab(Patient patient, DateTime when, string test, string result, string note)
    {
        var lab = new Laboratory
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            TestName = test,
            Result = result,
            Note = note
        };
        Stamp(lab, when);
        return lab;
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
        int? quantity,
        string? note = null)
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
            QuantityDispensed = quantity,
            DispensationNote = note
        };
        Stamp(rx, when);
        return rx;
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
        entity.CreatedBy = "sample-seed";
        entity.CreatedDate = created;
        entity.UpdatedBy = "sample-seed";
        entity.UpdatedDate = created;
        entity.Archived = false;
    }
}
