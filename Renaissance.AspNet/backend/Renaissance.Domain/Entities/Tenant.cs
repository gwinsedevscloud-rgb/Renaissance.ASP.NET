namespace Renaissance.Domain.Entities;

/// <summary>
/// An organization (hospital / NGO) that uses MedReach in isolation.
/// </summary>
public class Tenant
{
    public static readonly Guid DefaultTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public const string DefaultTenantCode = "medreach";
    public const string PlatformOrganizationCode = "platform";

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Short code entered at login (unique, lowercase).</summary>
    public string Code { get; set; } = string.Empty;
    public string? FacilityName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
