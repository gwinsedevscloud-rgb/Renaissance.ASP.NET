namespace Renaissance.Domain.Entities;

/// <summary>
/// Idempotency receipt for offline field sync (tablet → server).
/// </summary>
public class FieldSyncReceipt
{
    public Guid ClientRecordId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public Guid ServerId { get; set; }
    public string? ClientNumber { get; set; }
    public DateTime SyncedAtUtc { get; set; }
}
