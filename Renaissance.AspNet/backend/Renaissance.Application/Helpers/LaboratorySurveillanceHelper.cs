namespace Renaissance.Application.Helpers;

public static class LaboratorySurveillanceHelper
{
    public static bool IsMalariaTest(string? testName) =>
        ContainsAny(testName, "malaria", "mp ", "(mp)", "rdt");

    public static bool IsHivTest(string? testName) =>
        ContainsAny(testName, "hiv", "rapid test", "antibody");

    public static bool IsTbTest(string? testName) =>
        ContainsAny(testName, "tb", "tuberculosis", "genexpert", "afb", "sputum");

    public static bool IsPositiveResult(string? testName, string? result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return false;
        }

        var value = result.Trim();
        if (IsNegativeResult(value))
        {
            return false;
        }

        if (IsMalariaTest(testName))
        {
            return ContainsAny(value, "positive", "detected", "seen");
        }

        if (IsHivTest(testName))
        {
            return ContainsAny(value, "positive", "reactive", "detected");
        }

        if (IsTbTest(testName))
        {
            return ContainsAny(value, "positive", "detected", "mtb detected", "2+", "3+", "1+");
        }

        return ContainsAny(value, "positive", "reactive", "detected");
    }

    public static string SurveillanceCategory(string? testName)
    {
        if (IsMalariaTest(testName))
        {
            return "Malaria";
        }

        if (IsHivTest(testName))
        {
            return "HIV";
        }

        if (IsTbTest(testName))
        {
            return "TB";
        }

        return "Other";
    }

    private static bool IsNegativeResult(string value) =>
        ContainsAny(value, "negative", "non-reactive", "not detected", "within normal", "normal", "nil");

    private static bool ContainsAny(string? source, params string[] terms)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        var haystack = source.ToLowerInvariant();
        return terms.Any(term => haystack.Contains(term, StringComparison.Ordinal));
    }
}
