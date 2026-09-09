using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public interface IOutreachModeService
{
    Task<bool> IsOutreachModuleEnabledAsync(CancellationToken cancellationToken = default);
    Task<bool> IsOutreachModeAsync(CancellationToken cancellationToken = default);
    Task<CareProgramSummaryDto?> GetActiveProgramAsync(CancellationToken cancellationToken = default);
    Task<OutreachStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<AppModule?> GetNextModuleAfterAsync(AppModule sourceModule, CancellationToken cancellationToken = default);
    IReadOnlyList<AppModule> GetFlowModules();
}
