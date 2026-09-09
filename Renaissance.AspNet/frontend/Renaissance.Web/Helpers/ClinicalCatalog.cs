namespace Renaissance.Web.Helpers;

public static class ClinicalCatalog
{
    public static IReadOnlyList<string> ConsultationDiagnoses { get; } =
    [
        "Uncomplicated malaria",
        "Essential hypertension",
        "Type 2 diabetes mellitus",
        "Upper respiratory tract infection",
        "Antenatal booking — early pregnancy",
        "Typhoid fever",
        "Pneumonia",
        "Urinary tract infection",
        "Peptic ulcer disease",
        "Sickle cell vaso-occlusive crisis"
    ];

    public static IReadOnlyList<string> ConsultationTreatments { get; } =
    [
        "Artemether/Lumefantrine",
        "Amlodipine 5 mg daily",
        "Metformin 500 mg twice daily",
        "Paracetamol and rest",
        "Folic acid and tetanus toxoid",
        "Ceftriaxone IM",
        "Amoxicillin/clavulanate",
        "Low-salt diet counselling",
        "Oral rehydration therapy",
        "Pain control and hydration"
    ];

    public static IReadOnlyList<string> ReferralServices { get; } =
    [
        "Laboratory",
        "Pharmacy",
        "Dental",
        "Ancillary",
        "Optometry",
        "Ophthalmology",
        "Triage"
    ];

    public static IReadOnlyList<DentalServiceGroup> DentalServiceGroups { get; } =
    [
        new("Routine Check Up", "bi-clipboard2-check",
        [
            "Comprehensive oral examination",
            "Professional cleaning"
        ]),
        new("Preventive Care", "bi-shield-check",
        [
            "Scaling and Polishing",
            "Fluoride treatments",
            "Sealants",
            "Oral hygiene education"
        ]),
        new("Restorative Services", "bi-wrench-adjustable",
        [
            "Fillings"
        ]),
        new("Oral Surgery", "bi-bandaid",
        [
            "Extractions"
        ]),
        new("Others", "bi-grid",
        [
            "Giving of toothbrushes"
        ])
    ];

    public static IReadOnlyList<string> DentalDiagnoses { get; } =
        DentalServiceGroups.Select(g => g.Category).ToArray();

    public static IReadOnlyList<string> DentalTreatments { get; } =
        DentalServiceGroups.SelectMany(g => g.Services).ToArray();

    public sealed record DentalServiceGroup(string Category, string Icon, IReadOnlyList<string> Services);

    public static IReadOnlyList<string> DentalOtherDiagnoses { get; } =
    [
        "ACUTE PERIODONTITIS",
        "Acute pericoronitis",
        "BLEEDING GUM",
        "CHRONIC PERIODONTITIS",
        "Clinically healthy mouth",
        "Collapsed filling",
        "Cracked tooth syndrome",
        "DENTAL CARIES",
        "DENTAL PLAQUE",
        "Dentoalveolar abscess on lower right 1st premolar",
        "Denture",
        "Enamel hypoplasi",
        "Erupting incisors",
        "Erupting molars",
        "Erupting upper left 3",
        "FETOR ORIS",
        "Fractured tooth",
        "GINGIVAL POCKET",
        "GINGIVAL RECESSION",
        "Missing tooth",
        "Mobile tooth",
        "Pericoronitis 2° imparted third molar",
        "Periodontal abscess",
        "Recurrent Pericoronitis left last molar",
        "Retain destinies tooth; Lingual emptying incisor",
        "Stains",
        "Supernumerary teeth",
        "TOOTH SENSITIVITY",
        "Toothwear lesion",
        "Others"
    ];

    public static IReadOnlyList<string> DentalDispensedItems { get; } =
    [
        "Toothbrush",
        "Dental filling material",
        "Fluoride varnish",
        "Sealant material",
        "Chlorhexidine mouthwash",
        "Ibuprofen 400 mg",
        "Amoxicillin 500 mg"
    ];

    public static IReadOnlyList<string> AncillaryServices { get; } =
    [
        "Antenatal counselling",
        "Nutrition education",
        "Family planning counselling",
        "Immunization — TT",
        "Health talk — hygiene",
        "HIV prevention counselling",
        "Adolescent health talk",
        "Postnatal follow-up"
    ];

    public static IReadOnlyList<string> PregnancyStatuses { get; } =
    [
        "Not applicable",
        "Pregnant — first trimester (≤12 weeks)",
        "Pregnant — second trimester (13–27 weeks)",
        "Pregnant — third trimester (≥28 weeks)",
        "Postpartum — within 6 weeks",
        "Trying to conceive"
    ];

    public static IReadOnlyList<string> VisualAcuityOptions { get; } =
    [
        "6/6", "6/9", "6/12", "6/18", "6/24", "6/36", "6/60", "3/60", "CF", "HM", "LP", "NLP"
    ];

    public static IReadOnlyList<string> EyeClinicDiagnoses { get; } =
    [
        "ALLERGIC CONJUNCTIVITIS",
        "ASTIGMATISM",
        "BACTERIAL CONJUNCTIVITIS",
        "CORNEA OPACITY",
        "DRY EYE",
        "FOREIGN BODY",
        "GLAUCOMA",
        "HYPEROPIA",
        "IMMATURE CATARACT",
        "MATURE CATARACT",
        "MYOPIA",
        "PRESBYOPIA",
        "PTERYGIUM",
        "Vernal conjunctivitis"
    ];

    public static IReadOnlyList<string> EyeClinicTreatments { get; } =
    [
        "MEDICATION",
        "REFRACTION",
        "SURGERY"
    ];

    public static IReadOnlyList<string> EyeClinicServices { get; } =
    [
        "Eye Health Education",
        "Visual Acuity testing",
        "Ophthalmoscopy",
        "Refraction and dispensing of reading glasses",
        "Glaucoma screening and management",
        "Cataract screening and referral",
        "Management of minor ocular conditions"
    ];

    public static IReadOnlyList<string> EyeClinicMedications { get; } =
    [
        "Chloramphenicol eye drop",
        "Antallerg eye drop",
        "Timolol eye drop",
        "Hypromellose eyedrop",
        "Bet-N eyedrop",
        "Ivyflur eye drop"
    ];

    public static IReadOnlyList<string> OphthalmologyDiagnoses { get; } =
        EyeClinicDiagnoses;

    public static IReadOnlyList<string> OphthalmologyTreatments { get; } =
        EyeClinicTreatments;

    public static IReadOnlyList<string> OphthalmologySurgeries { get; } =
    [
        "Cataract surgery — left eye",
        "Cataract surgery — right eye",
        "Trabeculectomy",
        "Pterygium excision",
        "YAG capsulotomy"
    ];

    public static IReadOnlyList<string> TriagePresentingComplaints { get; } =
    [
        "Fever and chills",
        "Cough and chest pain",
        "Headache and weakness",
        "Antenatal visit",
        "Eye pain and blurred vision",
        "Tooth pain",
        "Abdominal pain",
        "Road traffic injury review"
    ];
}
