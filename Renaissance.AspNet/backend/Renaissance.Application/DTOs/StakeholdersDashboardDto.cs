namespace Renaissance.Application.DTOs;

public class StakeholdersDashboardDto
{
    public DateTime GeneratedAt { get; set; }
    public StakeholdersKpiSummary Kpis { get; set; } = new();
    public List<ModuleMetricDto> ModuleMetrics { get; set; } = [];
    public List<DailyCountDto> ActivityTrend { get; set; } = [];
    public List<DailyCountDto> ClientRegistrationTrend { get; set; } = [];
    public List<ForecastPointDto> ActivityForecast { get; set; } = [];
    public List<ForecastPointDto> ClientForecast { get; set; } = [];
    public List<LabelCountDto> SexDistribution { get; set; } = [];
    public List<LabelCountDto> AgeGroupDistribution { get; set; } = [];
    public List<LabelCountDto> PharmacyStatusBreakdown { get; set; } = [];
    public LabSurveillanceSummaryDto LabSurveillance { get; set; } = new();
    public PregnancySurveillanceSummaryDto PregnancySurveillance { get; set; } = new();
    public OperationalInsightsDto Insights { get; set; } = new();
}

public class LabSurveillanceSummaryDto
{
    public int TotalLabTests { get; set; }
    public int MalariaPositives { get; set; }
    public int HivPositives { get; set; }
    public int TbPositives { get; set; }
    public int TotalPriorityPositives { get; set; }
    public double PriorityPositivityRate { get; set; }
    public List<LabSurveillanceAlertDto> RecentAlerts { get; set; } = [];
    public List<LabelCountDto> PriorityTestBreakdown { get; set; } = [];
}

public class LabSurveillanceAlertDto
{
    public string PatientName { get; set; } = string.Empty;
    public string ClientNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime? RecordedAt { get; set; }
}

public class PregnancySurveillanceSummaryDto
{
    public int PregnantWomenTracked { get; set; }
    public int WithLabResults { get; set; }
    public List<PregnancyLabCaseDto> Cases { get; set; } = [];
}

public class PregnancyLabCaseDto
{
    public string PatientName { get; set; } = string.Empty;
    public string ClientNumber { get; set; } = string.Empty;
    public string PregnancyStatus { get; set; } = string.Empty;
    public List<PregnancyLabResultDto> LabResults { get; set; } = [];
}

public class PregnancyLabResultDto
{
    public string TestName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class StakeholdersKpiSummary
{
    public int TotalClients { get; set; }
    public int NewClientsThisMonth { get; set; }
    public int TotalEncounters { get; set; }
    public int EncountersThisMonth { get; set; }
    public double AvgEncountersPerClient { get; set; }
    public int PendingPharmacyOrders { get; set; }
    public double PharmacyDispenseRate { get; set; }
    public int ActiveModulesUsed { get; set; }
    public double MonthOverMonthGrowthPercent { get; set; }
}

public class ModuleMetricDto
{
    public string Module { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Total { get; set; }
    public int ThisMonth { get; set; }
    public int LastMonth { get; set; }
    public double SharePercent { get; set; }
}

public class DailyCountDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class ForecastPointDto
{
    public DateOnly Date { get; set; }
    public double Projected { get; set; }
    public bool IsForecast { get; set; }
}

public class LabelCountDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class OperationalInsightsDto
{
    public string BusiestModule { get; set; } = string.Empty;
    public string FastestGrowingModule { get; set; } = string.Empty;
    public double AvgDailyEncounters { get; set; }
    public double ProjectedMonthlyClients { get; set; }
    public double ProjectedMonthlyEncounters { get; set; }
    public string Summary { get; set; } = string.Empty;
}
