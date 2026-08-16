using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class StakeholdersDashboardService : IStakeholdersDashboardService
{
    private const int TrendDays = 30;
    private const int ForecastDays = 14;
    private const int ForecastBasisDays = 14;

    private readonly IApplicationDbContext _db;

    public StakeholdersDashboardService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StakeholdersDashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);
        var trendStart = now.Date.AddDays(-(TrendDays - 1));

        var totalClients = await _db.Patients.AsNoTracking().CountAsync(p => !p.Archived, cancellationToken);
        var newClientsThisMonth = await _db.Patients.AsNoTracking()
            .CountAsync(p => !p.Archived && p.CreatedDate >= monthStart, cancellationToken);
        var newClientsLastMonth = await _db.Patients.AsNoTracking()
            .CountAsync(p => !p.Archived && p.CreatedDate >= lastMonthStart && p.CreatedDate < monthStart, cancellationToken);

        var moduleCounts = await GetModuleCountsAsync(monthStart, lastMonthStart, cancellationToken);
        var totalEncounters = moduleCounts.Sum(m => m.Total);
        var encountersThisMonth = moduleCounts.Sum(m => m.ThisMonth);
        var encountersLastMonth = moduleCounts.Sum(m => m.LastMonth);

        var pendingPharmacy = await _db.PharmacyPrescriptions.AsNoTracking()
            .CountAsync(p => !p.Archived && p.Status == DispensationStatus.PENDING, cancellationToken);
        var dispensedPharmacy = await _db.PharmacyPrescriptions.AsNoTracking()
            .CountAsync(p => !p.Archived && p.Status == DispensationStatus.DISPENSED, cancellationToken);
        var totalPharmacy = pendingPharmacy + dispensedPharmacy + await _db.PharmacyPrescriptions.AsNoTracking()
            .CountAsync(p => !p.Archived && p.Status == DispensationStatus.DECLINED, cancellationToken);

        var clientTrend = await GetDailyPatientCountsAsync(trendStart, cancellationToken);
        var activityTrend = await GetDailyActivityCountsAsync(trendStart, cancellationToken);

        var activityForecast = BuildForecast(activityTrend, today, ForecastDays);
        var clientForecast = BuildForecast(clientTrend, today, ForecastDays);

        var sexDistribution = await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived && p.Sex != null && p.Sex != "")
            .GroupBy(p => p.Sex!)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var ageGroups = await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived)
            .Select(p => p.AgeUnit == "Years" ? p.Age :
                p.AgeUnit == "Months" ? p.Age / 12 :
                p.Age / 365)
            .ToListAsync(cancellationToken);

        var pharmacyBreakdown = await _db.PharmacyPrescriptions.AsNoTracking()
            .Where(p => !p.Archived)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var avgEncounters = totalClients > 0 ? Math.Round((double)totalEncounters / totalClients, 1) : 0;
        var momGrowth = encountersLastMonth > 0
            ? Math.Round((encountersThisMonth - encountersLastMonth) / (double)encountersLastMonth * 100, 1)
            : encountersThisMonth > 0 ? 100 : 0;

        var busiest = moduleCounts.OrderByDescending(m => m.Total).FirstOrDefault();
        var fastestGrowing = moduleCounts
            .OrderByDescending(m => m.ThisMonth - m.LastMonth)
            .FirstOrDefault();

        var avgDaily = activityTrend.Count > 0
            ? Math.Round(activityTrend.Average(d => d.Count), 1)
            : 0;

        var projectedMonthlyEncounters = Math.Round(avgDaily * 30, 0);
        var avgDailyClients = clientTrend.Count > 0 ? clientTrend.Average(d => d.Count) : 0;
        var projectedMonthlyClients = Math.Round(avgDailyClients * 30, 0);

        return new StakeholdersDashboardDto
        {
            GeneratedAt = now,
            Kpis = new StakeholdersKpiSummary
            {
                TotalClients = totalClients,
                NewClientsThisMonth = newClientsThisMonth,
                TotalEncounters = totalEncounters,
                EncountersThisMonth = encountersThisMonth,
                AvgEncountersPerClient = avgEncounters,
                PendingPharmacyOrders = pendingPharmacy,
                PharmacyDispenseRate = totalPharmacy > 0
                    ? Math.Round(dispensedPharmacy / (double)totalPharmacy * 100, 1)
                    : 0,
                ActiveModulesUsed = moduleCounts.Count(m => m.Total > 0),
                MonthOverMonthGrowthPercent = momGrowth
            },
            ModuleMetrics = moduleCounts,
            ActivityTrend = activityTrend,
            ClientRegistrationTrend = clientTrend,
            ActivityForecast = activityForecast,
            ClientForecast = clientForecast,
            SexDistribution = ToLabelCounts(sexDistribution.Select(x => (x.Label, x.Count)), totalClients),
            AgeGroupDistribution = ToAgeGroups(ageGroups, totalClients),
            PharmacyStatusBreakdown = ToLabelCounts(
                pharmacyBreakdown.Select(x => (x.Status.ToString(), x.Count)),
                totalPharmacy),
            Insights = new OperationalInsightsDto
            {
                BusiestModule = busiest?.Module ?? "—",
                FastestGrowingModule = fastestGrowing?.Module ?? "—",
                AvgDailyEncounters = avgDaily,
                ProjectedMonthlyClients = projectedMonthlyClients,
                ProjectedMonthlyEncounters = projectedMonthlyEncounters,
                Summary = BuildSummary(totalClients, encountersThisMonth, momGrowth, pendingPharmacy, projectedMonthlyEncounters)
            }
        };
    }

    private async Task<List<ModuleMetricDto>> GetModuleCountsAsync(
        DateTime monthStart,
        DateTime lastMonthStart,
        CancellationToken ct)
    {
        var modules = new (string Name, string Icon, Func<Task<(int Total, int ThisMonth, int LastMonth)>> Query)[]
        {
            ("Triage", "bi-heart-pulse", () => CountModuleAsync(_db.Triages, monthStart, lastMonthStart, ct)),
            ("Consultations", "bi-clipboard2-pulse", () => CountModuleAsync(_db.Consultations, monthStart, lastMonthStart, ct)),
            ("Pharmacy", "bi-capsule", () => CountModuleAsync(_db.PharmacyPrescriptions, monthStart, lastMonthStart, ct)),
            ("Laboratory", "bi-droplet", () => CountModuleAsync(_db.Laboratories, monthStart, lastMonthStart, ct)),
            ("Dental", "bi-emoji-smile", () => CountModuleAsync(_db.DentalConsultations, monthStart, lastMonthStart, ct, includeArchived: true)),
            ("Ancillary", "bi-plus-circle", () => CountModuleAsync(_db.Ancillaries, monthStart, lastMonthStart, ct, includeArchived: true)),
            ("Optometrist", "bi-eye", () => CountModuleAsync(_db.Optometrists, monthStart, lastMonthStart, ct)),
            ("Ophthalmologist", "bi-eye-fill", () => CountModuleAsync(_db.Ophthalmologists, monthStart, lastMonthStart, ct))
        };

        var results = new List<ModuleMetricDto>();
        foreach (var (name, icon, query) in modules)
        {
            var (total, thisMonth, lastMonth) = await query();
            results.Add(new ModuleMetricDto
            {
                Module = name,
                Icon = icon,
                Total = total,
                ThisMonth = thisMonth,
                LastMonth = lastMonth
            });
        }

        var grandTotal = results.Sum(r => r.Total);
        foreach (var r in results)
        {
            r.SharePercent = grandTotal > 0 ? Math.Round(r.Total / (double)grandTotal * 100, 1) : 0;
        }

        return results;
    }

    private static async Task<(int Total, int ThisMonth, int LastMonth)> CountModuleAsync<T>(
        IQueryable<T> query,
        DateTime monthStart,
        DateTime lastMonthStart,
        CancellationToken ct,
        bool includeArchived = false) where T : Domain.Entities.AuditEntity
    {
        if (!includeArchived)
        {
            query = query.Where(e => !e.Archived);
        }

        var total = await query.CountAsync(ct);
        var thisMonth = await query.CountAsync(e => e.CreatedDate >= monthStart, ct);
        var lastMonth = await query.CountAsync(
            e => e.CreatedDate >= lastMonthStart && e.CreatedDate < monthStart, ct);
        return (total, thisMonth, lastMonth);
    }

    private async Task<List<DailyCountDto>> GetDailyPatientCountsAsync(DateTime start, CancellationToken ct)
    {
        var raw = await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived && p.CreatedDate >= start)
            .GroupBy(p => p.CreatedDate!.Value.Date)
            .Select(g => new DailyCountDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Count = g.Count()
            })
            .ToListAsync(ct);

        return FillDateRange(raw, DateOnly.FromDateTime(start), DateOnly.FromDateTime(DateTime.UtcNow.Date));
    }

    private async Task<List<DailyCountDto>> GetDailyActivityCountsAsync(DateTime start, CancellationToken ct)
    {
        var dates = new Dictionary<DateOnly, int>();

        await AccumulateDailyAsync(_db.Triages.Where(t => !t.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.Consultations.Where(c => !c.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.PharmacyPrescriptions.Where(p => !p.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.Laboratories.Where(l => !l.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.DentalConsultations, start, dates, ct);
        await AccumulateDailyAsync(_db.Ancillaries, start, dates, ct);
        await AccumulateDailyAsync(_db.Optometrists.Where(o => !o.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.Ophthalmologists.Where(o => !o.Archived), start, dates, ct);
        await AccumulateDailyAsync(_db.Patients.Where(p => !p.Archived), start, dates, ct);

        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var result = new List<DailyCountDto>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            dates.TryGetValue(d, out var count);
            result.Add(new DailyCountDto { Date = d, Count = count });
        }

        return result;
    }

    private static async Task AccumulateDailyAsync<T>(
        IQueryable<T> query,
        DateTime start,
        Dictionary<DateOnly, int> dates,
        CancellationToken ct) where T : Domain.Entities.AuditEntity
    {
        var daily = await query
            .Where(e => e.CreatedDate >= start)
            .GroupBy(e => e.CreatedDate!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        foreach (var item in daily)
        {
            var date = DateOnly.FromDateTime(item.Date);
            dates[date] = dates.GetValueOrDefault(date) + item.Count;
        }
    }

    private static List<DailyCountDto> FillDateRange(List<DailyCountDto> raw, DateOnly start, DateOnly end)
    {
        var lookup = raw.ToDictionary(r => r.Date, r => r.Count);
        var result = new List<DailyCountDto>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            lookup.TryGetValue(d, out var count);
            result.Add(new DailyCountDto { Date = d, Count = count });
        }

        return result;
    }

    private static List<ForecastPointDto> BuildForecast(List<DailyCountDto> history, DateOnly today, int forecastDays)
    {
        var basis = history.TakeLast(ForecastBasisDays).ToList();
        if (basis.Count == 0)
        {
            return Enumerable.Range(1, forecastDays)
                .Select(i => new ForecastPointDto
                {
                    Date = today.AddDays(i),
                    Projected = 0,
                    IsForecast = true
                })
                .ToList();
        }

        var n = basis.Count;
        var sumX = basis.Select((_, i) => i).Sum();
        var sumY = basis.Sum(b => b.Count);
        var sumXY = basis.Select((b, i) => i * b.Count).Sum();
        var sumX2 = basis.Select((_, i) => i * i).Sum();
        var denom = n * sumX2 - sumX * sumX;
        var slope = denom != 0 ? (n * sumXY - sumX * sumY) / (double)denom : 0;
        var intercept = (sumY - slope * sumX) / n;

        var forecast = new List<ForecastPointDto>();
        for (var i = 1; i <= forecastDays; i++)
        {
            var projected = Math.Max(0, intercept + slope * (n - 1 + i));
            forecast.Add(new ForecastPointDto
            {
                Date = today.AddDays(i),
                Projected = Math.Round(projected, 1),
                IsForecast = true
            });
        }

        return forecast;
    }

    private static List<LabelCountDto> ToLabelCounts(IEnumerable<(string Label, int Count)> items, int total)
    {
        return items
            .OrderByDescending(i => i.Count)
            .Select(i => new LabelCountDto
            {
                Label = i.Label,
                Count = i.Count,
                Percent = total > 0 ? Math.Round(i.Count / (double)total * 100, 1) : 0
            })
            .ToList();
    }

    private static List<LabelCountDto> ToAgeGroups(List<int> agesInYears, int totalClients)
    {
        var groups = new (string Label, Func<int, bool> Match)[]
        {
            ("0–17", a => a < 18),
            ("18–35", a => a >= 18 && a <= 35),
            ("36–55", a => a >= 36 && a <= 55),
            ("56+", a => a >= 56)
        };

        return groups.Select(g => new LabelCountDto
        {
            Label = g.Label,
            Count = agesInYears.Count(g.Match),
            Percent = totalClients > 0
                ? Math.Round(agesInYears.Count(g.Match) / (double)totalClients * 100, 1)
                : 0
        }).ToList();
    }

    private static string BuildSummary(int clients, int encountersThisMonth, double momGrowth, int pendingPharmacy, double projectedEncounters)
    {
        var growthText = momGrowth >= 0 ? $"up {momGrowth}%" : $"down {Math.Abs(momGrowth)}%";
        var pharmacyNote = pendingPharmacy > 0
            ? $" {pendingPharmacy} pharmacy order(s) pending dispensation."
            : " Pharmacy queue is clear.";
        return $"The platform serves {clients} registered patient(s) with {encountersThisMonth} encounter(s) this month ({growthText} vs last month). " +
               $"Based on recent activity, projected monthly volume is ~{projectedEncounters:0} encounters.{pharmacyNote}";
    }
}
