namespace Renaissance.Web.Models;

public class HospitalSettingsDto
{
    public string FacilityName { get; set; } = string.Empty;
    public string ClientNumberPrefix { get; set; } = string.Empty;
    public string WebAccessUrl { get; set; } = string.Empty;
    public string ApiAccessUrl { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateHospitalSettingsRequest
{
    public string FacilityName { get; set; } = string.Empty;
    public string ClientNumberPrefix { get; set; } = string.Empty;
    public string WebAccessUrl { get; set; } = string.Empty;
    public string ApiAccessUrl { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
}

public class PublicHospitalSettingsDto
{
    public string FacilityName { get; set; } = string.Empty;
}

public class DeploymentInfoDto
{
    public List<string> LocalIpAddresses { get; set; } = [];
    public int ApiPort { get; set; }
    public int WebPort { get; set; }
    public string BindAddress { get; set; } = string.Empty;
    public bool AllowLanAccess { get; set; }
    public bool LanReady { get; set; }
    public string SuggestedStaffUrl { get; set; } = string.Empty;
    public string SuggestedApiUrl { get; set; } = string.Empty;
    public string StaffAccessUrl { get; set; } = string.Empty;
    public List<string> SetupSteps { get; set; } = [];
}
