using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IStakeholdersDashboardService
{
    Task<StakeholdersDashboardDto> GetAsync(CancellationToken cancellationToken = default);
}
