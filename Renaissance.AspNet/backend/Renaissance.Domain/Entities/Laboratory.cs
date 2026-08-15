using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class Laboratory : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public string? TestName { get; set; }
    public string? Result { get; set; }
    public string? Note { get; set; }
}
