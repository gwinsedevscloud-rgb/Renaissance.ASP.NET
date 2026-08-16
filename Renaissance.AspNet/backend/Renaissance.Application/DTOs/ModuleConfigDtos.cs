using Renaissance.Domain.Enums;

namespace Renaissance.Application.DTOs;

public class ModuleServiceDescriptorDto
{
    public AppModule Module { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public bool IsEnabled { get; set; }
    public bool SupportsReferrals { get; set; }
    public int SortOrder { get; set; }
}

public class ModuleReferralLinkDto
{
    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
}

public class HospitalModulesConfigDto
{
    public List<ModuleServiceDescriptorDto> Services { get; set; } = [];
    public List<ModuleReferralLinkDto> ReferralLinks { get; set; } = [];
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateHospitalModulesRequest
{
    public List<AppModule> EnabledModules { get; set; } = [];
    public List<ModuleReferralLinkDto> ReferralLinks { get; set; } = [];
}

public class HospitalModulesStateDto
{
    public List<AppModule> EnabledModules { get; set; } = [];
    public List<ModuleReferralLinkDto> ReferralLinks { get; set; } = [];
}
