using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class Ancillary : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public List<string> Services { get; set; } = new List<string>();
    public string? PregnancyStatus { get; set; }
}
