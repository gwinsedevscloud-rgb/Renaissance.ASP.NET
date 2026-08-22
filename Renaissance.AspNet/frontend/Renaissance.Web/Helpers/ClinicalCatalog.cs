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

    public static IReadOnlyList<string> DentalDiagnoses { get; } =
    [
        "Dental caries — lower molar",
        "Gingivitis",
        "Pericoronitis",
        "Acute apical periodontitis",
        "Partially erupted third molar",
        "Dental abscess"
    ];

    public static IReadOnlyList<string> DentalTreatments { get; } =
    [
        "Extraction planned",
        "Scaling and polishing",
        "Temporary filling",
        "Incision and drainage",
        "Analgesia and review",
        "Operculectomy"
    ];

    public static IReadOnlyList<string> DentalDispensedItems { get; } =
    [
        "Tooth extraction",
        "Dental filling material",
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

    public static IReadOnlyList<string> OphthalmologyDiagnoses { get; } =
    [
        "Immature cataract — OS",
        "Primary open-angle glaucoma",
        "Allergic conjunctivitis",
        "Diabetic retinopathy — NPDR",
        "Pterygium",
        "Refractive error — high myopia"
    ];

    public static IReadOnlyList<string> OphthalmologyTreatments { get; } =
    [
        "Phacoemulsification recommended",
        "Timolol eye drops",
        "Artificial tears",
        "Laser peripheral iridotomy",
        "Refer for low vision aids",
        "Observation and review"
    ];

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
