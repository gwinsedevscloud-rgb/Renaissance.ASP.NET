namespace Renaissance.Web.Helpers;

public sealed record LaboratoryTestOption(
    string Name,
    IReadOnlyList<string> StandardResults,
    string? DefaultNote = null);

public static class LaboratoryTestCatalog
{
    public static IReadOnlyList<LaboratoryTestOption> Tests { get; } =
    [
        new("Malaria Parasite (MP)", ["Positive", "Negative"], "Thick/thin film microscopy."),
        new("Malaria RDT", ["Positive", "Negative"], "Rapid diagnostic test."),
        new("HIV Rapid Test", ["Reactive (Positive)", "Non-reactive"], "Screening test — confirm if reactive."),
        new("HIV Rapid Antibody Test", ["Reactive (Positive)", "Non-reactive"], "Antibody screening."),
        new("CD4 Count", ["350 cells/µL", "412 cells/µL", "520 cells/µL"], "Immunology result."),
        new("TB GeneXpert MTB/RIF", ["MTB Detected, RIF Sensitive", "MTB Not Detected"], "Molecular TB test."),
        new("Sputum AFB Smear", ["Positive (1+)", "Positive (2+)", "Negative"], "Acid-fast bacilli smear."),
        new("Haemoglobin (Hb)", ["9.8 g/dL", "10.6 g/dL", "11.2 g/dL", "12.5 g/dL"], "Anaemia screening."),
        new("Syphilis RPR", ["Reactive", "Non-reactive"], "Antenatal syphilis screen."),
        new("Urinalysis", ["Normal", "Protein trace", "Glucose present"], "Routine antenatal urinalysis."),
        new("Fasting Blood Sugar", ["5.4 mmol/L", "6.8 mmol/L", "8.4 mmol/L"], "Diabetes screening."),
        new("Full Blood Count", ["Within normal limits", "Leucocytosis", "Anaemia pattern"], "General haematology.")
    ];

    public static LaboratoryTestOption? FindTest(string? testName) =>
        Tests.FirstOrDefault(t => string.Equals(t.Name, testName, StringComparison.OrdinalIgnoreCase));
}
