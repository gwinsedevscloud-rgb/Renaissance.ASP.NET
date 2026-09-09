namespace Renaissance.Web.Helpers;

public sealed record LaboratoryTestOption(
    string Name,
    IReadOnlyList<string> StandardResults,
    string? DefaultNote = null);

/// <summary>
/// Lab tests used in Nigerian outreach / hospital triage workflows.
/// </summary>
public static class LaboratoryTestCatalog
{
    public const string OtherKey = "__other__";

    public static IReadOnlyList<string> TestNames { get; } =
    [
        "Fasting Blood Sugar",
        "Random Blood Sugar",
        "Malaria Parasite_RDT",
        "Hepatitis B",
        "Hepatitis C",
        "HIV Test",
        "Cholesterol",
        "PSA"
    ];

    public static IReadOnlyList<LaboratoryTestOption> Tests { get; } =
        TestNames.Select(n => new LaboratoryTestOption(n, [])).ToList();

    public static LaboratoryTestOption? FindTest(string? testName) =>
        Tests.FirstOrDefault(t => string.Equals(t.Name, testName, StringComparison.OrdinalIgnoreCase));

    public static bool IsKnownTest(string? testName) => FindTest(testName) is not null;
}
