using Renaissance.Web.Models;

namespace Renaissance.Web.Services;

public sealed class HospitalModuleStateService
{
    private HashSet<AppModule> _enabled = [];
    private HashSet<(AppModule Source, AppModule Target)> _links = [];

    public event Action? Changed;

    public bool IsLoaded { get; private set; }

    public bool IsEnabled(AppModule module)
    {
        if (module is AppModule.Clients or AppModule.ClientDashboard or AppModule.Administration)
        {
            return true;
        }

        return _enabled.Contains(module);
    }

    public IReadOnlyList<AppModule> EnabledModules => _enabled.OrderBy(m => (int)m).ToList();

    public async Task LoadAsync(RenaissanceApiClient api, CancellationToken ct = default)
    {
        var state = await api.GetActiveHospitalModulesAsync(ct);
        Apply(state);
    }

    public void Apply(HospitalModulesStateDto state)
    {
        _enabled = state.EnabledModules.ToHashSet();
        _links = state.ReferralLinks
            .Select(l => (l.SourceModule, l.TargetModule))
            .ToHashSet();
        IsLoaded = true;
        Changed?.Invoke();
    }

    public bool CanRefer(AppModule source, AppModule target)
        => source != target && _links.Contains((source, target));

    public IEnumerable<ModuleNavItem> GetReferralTargets(AppModule source)
        => ModuleNav.ReferralTargets
            .Where(item => CanRefer(source, item.Module));

    public IEnumerable<ModuleNavItem> FilterNavItems(IEnumerable<ModuleNavItem> items)
        => items.Where(item => IsEnabled(item.Module));
}
