using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class Optometrist : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public bool? GlassesDispensed { get; set; }
    public bool? Referred { get; set; }
}
