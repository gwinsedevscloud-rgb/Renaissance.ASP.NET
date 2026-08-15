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
    public OperationalInsightsDto Insights { get; set; } = new();
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
