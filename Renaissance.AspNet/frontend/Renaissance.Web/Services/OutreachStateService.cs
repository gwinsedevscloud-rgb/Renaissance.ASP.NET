using Renaissance.Web.Models;

namespace Renaissance.Web.Services;

public sealed class OutreachStateService
{
    private OutreachStatusDto _status = new();

    public event Action? Changed;

    public bool IsLoaded { get; private set; }

    public OutreachStatusDto Status => _status;

    public bool IsOutreachMode => _status.IsOutreachMode;

    public bool OutreachModuleEnabled => _status.OutreachModuleEnabled;

    public CareProgramSummaryDto? ActiveProgram => _status.ActiveProgram;

    public IReadOnlyList<CareProgramSummaryDto> ActiveSecondaryPrograms => _status.ActiveSecondaryPrograms;

    public async Task LoadAsync(RenaissanceApiClient api, CancellationToken ct = default)
    {
        _status = await api.GetOutreachStatusAsync(ct);
        IsLoaded = true;
        Changed?.Invoke();
    }

    public void Apply(OutreachStatusDto status)
    {
        _status = status;
        IsLoaded = true;
        Changed?.Invoke();
    }

    public bool IsFlowModule(AppModule module)
        => _status.FlowModules.Contains(module);

    public IEnumerable<ModuleNavItem> FilterModules(IEnumerable<ModuleNavItem> items)
    {
        if (!_status.IsOutreachMode)
        {
            return items;
        }

        return items.Where(m => _status.FlowModules.Contains(m.Module));
    }
}
