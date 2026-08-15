using Renaissance.Web.Models;

namespace Renaissance.Web.Helpers;

public static class ClientDisplayHelper
{
    public static string DisplayName(Patient patient)
        => DisplayName(patient.FullName, patient.Address);

    public static string DisplayName(string? fullName, string? address = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName.Trim();
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return "—";
        }

        var comma = address.IndexOf(',');
        return comma > 0 ? address[..comma].Trim() : address.Trim();
    }

    public static string Initials(Patient patient)
        => InitialsFromName(DisplayName(patient));

    public static string InitialsFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name == "—")
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
