namespace Renaissance.Web.Helpers;

public sealed record DrugOption(
    string Name,
    string Dosage,
    string Frequency,
    string Duration);

public static class DrugCatalog
{
    public static IReadOnlyList<string> Categories { get; } =
    [
        "Antimalarial",
        "Antihypertensive",
        "Antidiabetic",
        "Analgesic",
        "Antibiotic",
        "Supplement",
        "Antiretroviral",
        "Antitubercular",
        "Antenatal",
        "Antiglaucoma"
    ];

    private static readonly Dictionary<string, IReadOnlyList<DrugOption>> Catalog = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Antimalarial"] =
        [
            new("Artemether/Lumefantrine (Coartem)", "80/480 mg", "Twice daily", "3 days"),
            new("Artesunate-Amodiaquine", "100/270 mg", "Once daily", "3 days"),
            new("Quinine Sulphate", "600 mg", "Three times daily", "7 days")
        ],
        ["Antihypertensive"] =
        [
            new("Amlodipine", "5 mg", "Once daily", "30 days"),
            new("Lisinopril", "10 mg", "Once daily", "30 days"),
            new("Hydrochlorothiazide", "25 mg", "Once daily", "30 days")
        ],
        ["Antidiabetic"] =
        [
            new("Metformin", "500 mg", "Twice daily", "30 days"),
            new("Glibenclamide", "5 mg", "Once daily", "30 days"),
            new("Insulin NPH", "10 units", "Twice daily", "30 days")
        ],
        ["Analgesic"] =
        [
            new("Paracetamol", "500 mg", "Three times daily", "5 days"),
            new("Ibuprofen", "400 mg", "Three times daily", "5 days"),
            new("Tramadol", "50 mg", "Twice daily", "3 days")
        ],
        ["Antibiotic"] =
        [
            new("Amoxicillin", "500 mg", "Three times daily", "7 days"),
            new("Azithromycin", "500 mg", "Once daily", "3 days"),
            new("Ciprofloxacin", "500 mg", "Twice daily", "7 days")
        ],
        ["Supplement"] =
        [
            new("Folic Acid", "5 mg", "Once daily", "30 days"),
            new("Ferrous Sulphate + Folic Acid", "200/5 mg", "Once daily", "90 days"),
            new("Multivitamin", "One tablet", "Once daily", "30 days")
        ],
        ["Antiretroviral"] =
        [
            new("TDF/3TC/DTG (TLD)", "One tablet", "Once daily", "Continuous"),
            new("TDF/3TC/EFV", "One tablet", "Once daily at night", "Continuous"),
            new("AZT/3TC/NVP", "One tablet", "Twice daily", "Continuous")
        ],
        ["Antitubercular"] =
        [
            new("RHZE (4FDC)", "Fixed-dose combination", "Once daily", "2 months (intensive phase)"),
            new("Isoniazid + Rifampicin", "Fixed-dose combination", "Once daily", "4 months (continuation phase)"),
            new("Pyridoxine (Vitamin B6)", "25 mg", "Once daily", "6 months")
        ],
        ["Antenatal"] =
        [
            new("Tetanus Toxoid", "0.5 mL IM", "Single dose", "Once"),
            new("Ferrous Folate", "One tablet", "Once daily", "90 days"),
            new("Magnesium Sulphate", "4 g IV loading", "Single dose", "Once")
        ],
        ["Antiglaucoma"] =
        [
            new("Timolol eye drops", "0.5%", "Twice daily", "30 days"),
            new("Latanoprost eye drops", "0.005%", "Once daily at night", "30 days"),
            new("Brimonidine eye drops", "0.2%", "Twice daily", "30 days")
        ]
    };

    public static IReadOnlyList<DrugOption> GetDrugs(string? category) =>
        category is not null && Catalog.TryGetValue(category, out var drugs)
            ? drugs
            : Array.Empty<DrugOption>();

    public static DrugOption? FindDrug(string? category, string? drugName)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(drugName))
        {
            return null;
        }

        return GetDrugs(category).FirstOrDefault(d =>
            string.Equals(d.Name, drugName, StringComparison.OrdinalIgnoreCase));
    }
}
