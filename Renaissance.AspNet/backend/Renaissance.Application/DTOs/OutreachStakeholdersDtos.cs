namespace Renaissance.Application.DTOs;

public class OutreachStakeholdersDashboardDto
{
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRegistrations { get; set; }
    public int RegistrationsThisMonth { get; set; }
    public int? TargetBatchSize { get; set; }
    public List<LabelCountDto> StatusBreakdown { get; set; } = [];
    public List<LabelCountDto> SexDistribution { get; set; } = [];
    public List<LabelCountDto> AgeGroupDistribution { get; set; } = [];
    public List<DailyCountDto> RegistrationTrend { get; set; } = [];
}

public class StakeholdersOutreachOverviewDto
{
    public DateTime GeneratedAt { get; set; }
    public StakeholdersDashboardDto? FacilityOverview { get; set; }
    public List<OutreachStakeholdersDashboardDto> OutreachDashboards { get; set; } = [];
}
