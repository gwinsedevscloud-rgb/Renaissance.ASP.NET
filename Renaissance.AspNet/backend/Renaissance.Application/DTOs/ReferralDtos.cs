using Renaissance.Domain.Enums;

namespace Renaissance.Application.DTOs;

public class CreateReferralsRequest
{
    public Guid PatientId { get; set; }
    public AppModule SourceModule { get; set; }
    public Guid? SourceRecordId { get; set; }
    public List<AppModule> TargetModules { get; set; } = [];
    public string? Notes { get; set; }
    public ReferralPriority Priority { get; set; } = ReferralPriority.Routine;
}

public class ReassignReferralRequest
{
    public AppModule TargetModule { get; set; }
    public string? Notes { get; set; }
}

public class ReferralQueueItemDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string ClientNumber { get; set; } = string.Empty;
    public int? PatientAge { get; set; }
    public string? PatientAgeUnit { get; set; }
    public string? PatientSex { get; set; }
    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
    public ReferralStatus Status { get; set; }
    public ReferralPriority Priority { get; set; }
    public string? Notes { get; set; }
    public string? ReferredBy { get; set; }
    public string? AttendedBy { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? AttendedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int QueuePosition { get; set; }
    public int WaitMinutes { get; set; }
    public bool IsSlaBreached { get; set; }
}

public class ReferralNotificationDto
{
    public Guid ReferralId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string ClientNumber { get; set; } = string.Empty;
    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
    public ReferralPriority Priority { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class ReferralInboxItemDto
{
    public Guid ReferralId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string ClientNumber { get; set; } = string.Empty;
    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
    public ReferralStatus Status { get; set; }
    public ReferralPriority Priority { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int WaitMinutes { get; set; }
    public bool IsRead { get; set; }
}

public class ModuleReferralCountDto
{
    public AppModule Module { get; set; }
    public int PendingCount { get; set; }
}

public class PatientJourneyStepDto
{
    public Guid ReferralId { get; set; }
    public AppModule TargetModule { get; set; }
    public AppModule SourceModule { get; set; }
    public ReferralStatus Status { get; set; }
    public ReferralPriority Priority { get; set; }
    public string? ReferredBy { get; set; }
    public string? AttendedBy { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? AttendedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateReferralsResultDto
{
    public List<ReferralQueueItemDto> Created { get; set; } = [];
    public List<AppModule> SkippedDuplicates { get; set; } = [];
}
