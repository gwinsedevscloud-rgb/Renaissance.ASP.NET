namespace Renaissance.Domain.Entities;

/// <summary>
/// Field tablet that has pulled/pushed via api/field-sync.
/// </summary>
public class FieldDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceLabel { get; set; }
    public DateTime FirstSeenUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
    public DateTime? LastPullUtc { get; set; }
    public DateTime? LastPushUtc { get; set; }
    public string? LastActor { get; set; }
}
