using System.Text.Json.Serialization;
using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class PharmacyPrescription : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public string? DrugCategory { get; set; }
    public string? DrugName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public bool Dispensed { get; set; }
    public string? DispensationNote { get; set; }
    public DispensationStatus Status { get; set; } = DispensationStatus.PENDING;
    public int? QuantityDispensed { get; set; }
}
