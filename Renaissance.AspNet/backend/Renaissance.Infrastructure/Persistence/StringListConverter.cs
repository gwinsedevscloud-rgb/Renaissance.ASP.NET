using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Renaissance.Infrastructure.Persistence;

public class StringListConverter : ValueConverter<List<string>, string>
{
    public StringListConverter()
        : base(
            v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : (JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()))
    {
    }
}
