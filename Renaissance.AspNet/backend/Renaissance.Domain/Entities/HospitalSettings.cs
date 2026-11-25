namespace Renaissance.Domain.Entities;

public class HospitalSettings
{
    public const int SingletonId = 1;

    /// <summary>Database IDENTITY. Leave 0 for new rows so SQL Server generates the value.</summary>
    public int Id { get; set; }
    /// <summary>Owning organization. Default tenant uses the legacy singleton row.</summary>
    public Guid TenantId { get; set; } = Tenant.DefaultTenantId;
    public string FacilityName { get; set; } = "MedReach";
    public string ClientNumberPrefix { get; set; } = "ACH";
    public string WebAccessUrl { get; set; } = string.Empty;
    public string ApiAccessUrl { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public string? LanAccessPasswordHash { get; set; }
    public bool OutreachModuleEnabled { get; set; } = true;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    public Tenant? Tenant { get; set; }
}
