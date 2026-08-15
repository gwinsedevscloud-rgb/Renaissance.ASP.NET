namespace Renaissance.Web.ViewModels;

public class StakeholdersDashboardViewModel
{
    public DateTime GeneratedAt { get; set; }
    public StakeholdersKpiViewModel Kpis { get; set; } = new();
    public List<ModuleMetricViewModel> ModuleMetrics { get; set; } = [];
    public List<DailyCountViewModel> ActivityTrend { get; set; } = [];
    public List<DailyCountViewModel> ClientRegistrationTrend { get; set; } = [];
    public List<ForecastPointViewModel> ActivityForecast { get; set; } = [];
    public List<ForecastPointViewModel> ClientForecast { get; set; } = [];
    public List<LabelCountViewModel> SexDistribution { get; set; } = [];
    public List<LabelCountViewModel> AgeGroupDistribution { get; set; } = [];
    public List<LabelCountViewModel> PharmacyStatusBreakdown { get; set; } = [];
    public OperationalInsightsViewModel Insights { get; set; } = new();
}

public class StakeholdersKpiViewModel
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

public class ModuleMetricViewModel
{
    public string Module { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Total { get; set; }
    public int ThisMonth { get; set; }
    public int LastMonth { get; set; }
    public double SharePercent { get; set; }
}

public class DailyCountViewModel
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class ForecastPointViewModel
{
    public DateOnly Date { get; set; }
    public double Projected { get; set; }
    public bool IsForecast { get; set; }
}

public class LabelCountViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class OperationalInsightsViewModel
{
    public string BusiestModule { get; set; } = string.Empty;
    public string FastestGrowingModule { get; set; } = string.Empty;
    public double AvgDailyEncounters { get; set; }
    public double ProjectedMonthlyClients { get; set; }
    public double ProjectedMonthlyEncounters { get; set; }
    public string Summary { get; set; } = string.Empty;
}
