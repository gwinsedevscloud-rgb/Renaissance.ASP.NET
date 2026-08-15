namespace Renaissance.Web.Models;

public enum ReferralStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum ReferralPriority
{
    Emergency = 1,
    Urgent = 2,
    Routine = 3
}

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

public class ReferralQueueItem
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

public class ReferralNotification
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

public class ReferralInboxItem
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

public class ModuleReferralCount
{
    public AppModule Module { get; set; }
    public int PendingCount { get; set; }
}

public class PatientJourneyStep
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

public class CreateReferralsResult
{
    public List<ReferralQueueItem> Created { get; set; } = [];
    public List<AppModule> SkippedDuplicates { get; set; } = [];
}
