export interface StakeholdersDashboardDto {
    generatedAt: string;
    kpis: StakeholdersKpiSummary;
    moduleMetrics: ModuleMetric[];
    activityTrend: DailyCount[];
    clientRegistrationTrend: DailyCount[];
    activityForecast: ForecastPoint[];
    clientForecast: ForecastPoint[];
    sexDistribution: LabelCount[];
    ageGroupDistribution: LabelCount[];
    pharmacyStatusBreakdown: LabelCount[];
    insights: OperationalInsights;
}

export interface StakeholdersKpiSummary {
    totalClients: number;
    newClientsThisMonth: number;
    totalEncounters: number;
    encountersThisMonth: number;
    avgEncountersPerClient: number;
    pendingPharmacyOrders: number;
    pharmacyDispenseRate: number;
    activeModulesUsed: number;
    monthOverMonthGrowthPercent: number;
}

export interface ModuleMetric {
    module: string;
    icon: string;
    total: number;
    thisMonth: number;
    lastMonth: number;
    sharePercent: number;
}

export interface DailyCount {
    date: string;
    count: number;
}

export interface ForecastPoint {
    date: string;
    projected: number;
    isForecast: boolean;
}

export interface LabelCount {
    label: string;
    count: number;
    percent: number;
}

export interface OperationalInsights {
    busiestModule: string;
    fastestGrowingModule: string;
    avgDailyEncounters: number;
    projectedMonthlyClients: number;
    projectedMonthlyEncounters: number;
    summary: string;
}
