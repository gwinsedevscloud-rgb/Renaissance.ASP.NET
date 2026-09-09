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
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> Services { get; set; } = [];
    public List<string> Medications { get; set; } = [];
    public bool? GlassesDispensed { get; set; }
    public bool? Referred { get; set; }
}
