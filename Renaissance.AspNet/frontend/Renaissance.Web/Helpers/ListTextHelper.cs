namespace Renaissance.Web.Helpers;

public static class ListTextHelper
{
    public static string ToMultiline(IEnumerable<string>? values)
        => values is null ? string.Empty : string.Join(Environment.NewLine, values.Where(v => !string.IsNullOrWhiteSpace(v)));

    public static List<string> FromMultiline(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();
    }
}
