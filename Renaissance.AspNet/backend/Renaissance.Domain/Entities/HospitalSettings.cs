namespace Renaissance.Domain.Entities;

public class HospitalSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public string FacilityName { get; set; } = "Renaissance Hospital";
    public string ClientNumberPrefix { get; set; } = "ACH";
    public string WebAccessUrl { get; set; } = string.Empty;
    public string ApiAccessUrl { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public string? LanAccessPasswordHash { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
