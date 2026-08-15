using System.Text.Json.Serialization;
using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class Referral : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
    public Guid? SourceRecordId { get; set; }
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public ReferralPriority Priority { get; set; } = ReferralPriority.Routine;
    public string? Notes { get; set; }
    public string? AttendedBy { get; set; }
    public DateTime? AttendedDate { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CancelledReason { get; set; }
}
