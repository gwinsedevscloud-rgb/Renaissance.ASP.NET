using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class Ophthalmologist : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public List<string> Diagnoses { get; set; } = new List<string>();
    public List<string> Treatments { get; set; } = new List<string>();
    public List<string> Surgeries { get; set; } = new List<string>();
    public List<string> OthersDiagnosis { get; set; } = new List<string>();
    public List<string> OthersTreatment { get; set; } = new List<string>();
    public List<string> OtherSurgery { get; set; } = new List<string>();
    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public bool? GlassesDispensed { get; set; }
    public bool? Referred { get; set; }
}
