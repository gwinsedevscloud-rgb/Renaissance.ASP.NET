using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class Triage : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public bool? Diabetes { get; set; }
    public bool? Asthma { get; set; }
    public bool? SickleCell { get; set; }
    public bool? Smoking { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public double? Temperature { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
}
